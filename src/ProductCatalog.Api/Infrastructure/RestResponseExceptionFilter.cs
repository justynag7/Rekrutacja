using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using ProductCatalog.Api.Models.Api;

namespace ProductCatalog.Api.Infrastructure;

public sealed class RestResponseExceptionFilter : IExceptionFilter
{
    private readonly ILogger<RestResponseExceptionFilter> _logger;

    public RestResponseExceptionFilter(ILogger<RestResponseExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.ExceptionHandled)
        {
            return;
        }

        var restResponseType = GetRestResponseType(context);
        if (restResponseType is null)
        {
            return;
        }

        _logger.LogError(
            context.Exception,
            "Unhandled error in {Action} at {Path}",
            context.ActionDescriptor.DisplayName,
            context.HttpContext.Request.Path.Value);

        var message = context.Exception switch
        {
            InvalidOperationException or ArgumentException => context.Exception.Message,
            _ => "An unexpected error occurred."
        };

        context.Result = new ObjectResult(CreateErrorResult(restResponseType, message))
        {
            StatusCode = StatusCodes.Status200OK
        };
        context.ExceptionHandled = true;
    }

    private static Type? GetRestResponseType(ExceptionContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
        {
            return null;
        }

        var returnType = descriptor.MethodInfo.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            returnType = returnType.GetGenericArguments()[0];
        }

        if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(RestResponse<>))
        {
            return null;
        }

        return returnType;
    }

    private static object CreateErrorResult(Type restResponseType, string errorMessage)
    {
        var result = Activator.CreateInstance(restResponseType)
            ?? throw new InvalidOperationException($"Could not create {restResponseType.Name}.");

        restResponseType.GetProperty(nameof(RestResponse<object>.IsSuccess))!
            .SetValue(result, false);
        restResponseType.GetProperty(nameof(RestResponse<object>.ErrorMessage))!
            .SetValue(result, errorMessage);

        return result;
    }
}
