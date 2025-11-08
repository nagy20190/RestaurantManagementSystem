using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DeliveryManagementSystem.Middleware.YourApp
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var responseObj = new
            {
                Message = "An error occurred while processing the request.",
            };

            switch (exception)
            {
                case KeyNotFoundException knf:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    responseObj = new { Message = knf.Message ?? "Resource not found." };
                    break;

                case ArgumentNullException ane:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    responseObj = new { Message = ane.Message };
                    break;

                case NullReferenceException nre:
                    // NullReference indicates a server-side bug — return 500 but do not leak internals
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    responseObj = new { Message = "A server error occurred (null reference)." };
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    responseObj = new { Message = "An unexpected server error occurred." };
                    break;
            }

            var json = JsonSerializer.Serialize(responseObj);
            await context.Response.WriteAsync(json);
        }
    }
}

