using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DeliveryManagementSystem.Middleware.YourApp
{
    public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}", 
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            string message;

            switch (exception)
            {
                case KeyNotFoundException knf:
                    statusCode = StatusCodes.Status404NotFound;
                    message = knf.Message ?? "Resource not found.";
                    break;

                case UnauthorizedAccessException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = "You are not authorized to perform this action.";
                    break;

                case InvalidOperationException ioe:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = ioe.Message ?? "Invalid operation.";
                    break;

                case ArgumentNullException ane:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = ane.Message ?? "A required argument was null.";
                    break;

                case ArgumentException ae:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = ae.Message ?? "Invalid argument provided.";
                    break;

                case NullReferenceException:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "A server error occurred. Please try again later.";
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "An unexpected server error occurred. Please try again later.";
                    break;
            }

            context.Response.StatusCode = statusCode;

            var response = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                Path = context.Request.Path,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response, _jsonOptions);
            await context.Response.WriteAsync(json);
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}

