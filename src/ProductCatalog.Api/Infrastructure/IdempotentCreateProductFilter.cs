using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProductCatalog.Api.Infrastructure.Interfaces;
using ProductCatalog.Api.Models.Api;
using ProductCatalog.Api.Models.Api.Product;
using ProductCatalog.Api.Models.Idempotency;

namespace ProductCatalog.Api.Infrastructure;

public sealed class IdempotentCreateProductFilter : IAsyncActionFilter
{
    public const string HeaderName = "Idempotency-Key";

    private readonly IIdempotencyStore _store;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public IdempotentCreateProductFilter(IIdempotencyStore store)
    {
        _store = store;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!HttpMethods.IsPost(context.HttpContext.Request.Method))
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var keyValues) ||
            string.IsNullOrWhiteSpace(keyValues))
        {
            context.Result = new OkObjectResult(
                RestResponse<ProductInfo>.CreateErrorResponse(
                    $"Header {HeaderName} is required for POST /api/products."));
            return;
        }

        var rawKey = keyValues.ToString().Trim();
        if (rawKey.Length is < 8 or > 128 ||
            rawKey.Any(c => char.IsControl(c) || char.IsWhiteSpace(c)))
        {
            context.Result = new OkObjectResult(
                RestResponse<ProductInfo>.CreateErrorResponse(
                    "Idempotency-Key must be 8–128 characters without whitespace."));
            return;
        }

        if (!context.ActionArguments.TryGetValue("request", out var arg) ||
            arg is not CreateProductRequestInfo request)
        {
            context.Result = new OkObjectResult(
                RestResponse<ProductInfo>.CreateErrorResponse("Request body is required."));
            return;
        }

        var requestHash = ComputeHash(request);
        var outcome = _store.BeginOrGet(rawKey, requestHash);

        switch (outcome.Kind)
        {
            case IdempotencyOutcomeKind.Replay when outcome.Record is not null:
                context.HttpContext.Response.Headers["Idempotency-Replayed"] = "true";
                context.Result = new ObjectResult(
                    JsonSerializer.Deserialize<RestResponse<ProductInfo>>(outcome.Record.Body, JsonOptions))
                {
                    StatusCode = outcome.Record.StatusCode
                };
                return;

            case IdempotencyOutcomeKind.Conflict:
                context.Result = new OkObjectResult(
                    RestResponse<ProductInfo>.CreateErrorResponse(
                        "Idempotency-Key was already used with a different request body."));
                return;

            case IdempotencyOutcomeKind.InProgress:
                context.Result = new OkObjectResult(
                    RestResponse<ProductInfo>.CreateErrorResponse(
                        "A request with this Idempotency-Key is already in progress."));
                return;
        }

        ActionExecutedContext executed;
        try
        {
            executed = await next();
        }
        catch
        {
            _store.Abandon(rawKey);
            throw;
        }

        if (executed.Exception is not null && !executed.ExceptionHandled)
        {
            _store.Abandon(rawKey);
            return;
        }

        if (executed.Result is ObjectResult { Value: RestResponse<ProductInfo> response } objectResult
            && response.IsSuccess)
        {
            _store.Complete(rawKey, new IdempotencyRecord
            {
                RequestHash = requestHash,
                StatusCode = objectResult.StatusCode ?? StatusCodes.Status200OK,
                ContentType = "application/json",
                Body = JsonSerializer.Serialize(response, JsonOptions),
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
        }
        else
        {
            _store.Abandon(rawKey);
        }
    }

    private static string ComputeHash(CreateProductRequestInfo request)
    {
        var canonical = new
        {
            code = request.Code.Trim(),
            name = request.Name.Trim(),
            price = request.Price
        };
        var json = JsonSerializer.Serialize(canonical, JsonOptions);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
    }
}
