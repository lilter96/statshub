using System.Net;
using System.Text.Json;
using FluentValidation;

public class ApiExceptionMiddleware : IMiddleware
{
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(ILogger<ApiExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var result = JsonSerializer.Serialize(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Validation Failed",
                status = 400,
                errors
            });

            await context.Response.WriteAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new
            {
                title = "Internal Server Error",
                status = 500,
                detail = ex.Message
            });

            await context.Response.WriteAsync(result);
        }
    }
}
