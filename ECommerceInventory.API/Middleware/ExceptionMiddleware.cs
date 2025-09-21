using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace ECommerceInventory.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong: {ex.Message}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode status = HttpStatusCode.InternalServerError;
            string message = exception.Message; // আগে শুধু "Internal Server Error" ছিল
            string? stack = exception.StackTrace;

            context.Response.StatusCode = (int)status;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = message,
                StackTrace = stack
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}
