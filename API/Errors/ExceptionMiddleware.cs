using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace API.Errors;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private async Task HandleException(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "An error occurred.");

        var probleDetails = new ProblemDetails
        {
            Title = "A Problem occurred while processing your request, please try again later.",
            Detail = ex.Message,
            Status = StatusCodes.Status500InternalServerError,
            
        };


        await context.Response.WriteAsync(JsonConvert.SerializeObject(probleDetails));
    }
}
