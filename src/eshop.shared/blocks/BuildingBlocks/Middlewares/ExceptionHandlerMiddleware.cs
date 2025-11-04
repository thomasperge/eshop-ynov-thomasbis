using System.Text.Json;
using BuildingBlocks.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Middlewares;

/// <summary>
/// Represents the structured error response returned to the client in the event of
/// an exception during the processing of an HTTP request. This response contains
/// the HTTP status code, a message describing the error, and optional additional
/// data to provide contextual information about the error.
/// </summary>
public record ErrorResponse(int StatusCode, string Message, object? Data);

/// <summary>
/// Middleware component responsible for handling exceptions that occur during the
/// processing of HTTP requests. This middleware ensures that unhandled exceptions
/// are intercepted, logged, and appropriate responses are sent to the client.
/// </summary>
public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger, IHostEnvironment environment)
{

    /// <summary>
    /// Invokes the middleware logic to process the current HTTP context. This method intercepts
    /// exceptions thrown during the execution of the pipeline, handles them, and ensures an
    /// appropriate response is sent to the client.
    /// </summary>
    /// <param name="context">The HttpContext for the current request, containing information about the HTTP request and response.</param>
    /// <returns>A Task representing the asynchronous middleware operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            BusinessException ex => new ErrorResponse(400, ex.Message, environment.IsDevelopment() ? ex.StackTrace : null),
            ValidationException ex => 
                new ErrorResponse(StatusCodes.Status400BadRequest,"Validation failed", 
                    ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })),
            _ => new ErrorResponse(
                StatusCodes.Status500InternalServerError, 
                environment.IsDevelopment() ? exception.Message : "An error occurred",
                environment.IsDevelopment() ? new { 
                    Message = exception.Message, 
                    StackTrace = exception.StackTrace, 
                    InnerException = exception.InnerException?.Message 
                } : null),
        };
        
        logger.LogError(exception, "An unhandled exception occurred while processing the request.");
        
        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
    
}
