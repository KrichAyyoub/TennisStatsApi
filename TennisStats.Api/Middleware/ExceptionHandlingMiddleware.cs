using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TennisStats.Api.DTOs;
using TennisStats.Api.Exceptions;

namespace TennisStats.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                LogException(ex);
                await HandleExceptionAsync(context, ex);
            }
        }

        private void LogException(Exception ex)
        {
            switch (ex)
            {
                case PlayerNotFoundException:
                    _logger.LogWarning("Player not found exception: {Message}", ex.Message);
                    break;
                case ValidationException:
                case ArgumentException:
                    _logger.LogWarning("Validation or argument exception: {Message}", ex.Message);
                    break;
                default:
                    _logger.LogError(ex, "An unexpected error occurred while processing the request.");
                    break;
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = StatusCodes.Status500InternalServerError;
            var errorTitle = "Internal Server Error";
            var message = "An unexpected error occurred. Please try again later.";

            switch (exception)
            {
                case PlayerNotFoundException notFoundEx:
                    statusCode = StatusCodes.Status404NotFound;
                    errorTitle = "Not Found";
                    message = notFoundEx.Message;
                    break;

                case ValidationException valEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    errorTitle = "Bad Request";
                    message = valEx.Message;
                    break;

                case ArgumentException argEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    errorTitle = "Bad Request";
                    message = argEx.Message;
                    break;
            }

            context.Response.StatusCode = statusCode;

            var errorResponse = new ErrorResponseDto
            {
                StatusCode = statusCode,
                Error = errorTitle,
                Message = message
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonResult = JsonSerializer.Serialize(errorResponse, options);
            return context.Response.WriteAsync(jsonResult);
        }
    }
}
