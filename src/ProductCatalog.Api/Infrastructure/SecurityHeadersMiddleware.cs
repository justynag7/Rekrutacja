namespace ProductCatalog.Api.Infrastructure;

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
        headers["Content-Security-Policy"] =
                "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";

            if (context.Request.IsHttps)
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
