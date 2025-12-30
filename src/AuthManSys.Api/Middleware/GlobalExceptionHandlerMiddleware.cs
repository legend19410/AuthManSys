using System.Net;
using System.Text.Json;
using AuthManSys.Application.Common.Exceptions;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Domain.Enums;

namespace AuthManSys.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            Message = "An error occurred",
            Details = (string?)null
        };

        switch (exception)
        {
            case UnauthorizedException unauthorizedException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = new
                {
                    Message = unauthorizedException.Message,
                    Details = (string?)null
                };
                break;

            case NotFoundException notFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    Message = notFoundException.Message,
                    Details = (string?)null
                };
                break;

            case ArgumentException argumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    Message = argumentException.Message,
                    Details = (string?)null
                };
                break;

            case InvalidOperationException invalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    Message = invalidOperationException.Message,
                    Details = (string?)null
                };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                _logger.LogError(exception, "An unhandled exception occurred");

                // Log the error using activity service if available
                var activityLogService = context.RequestServices.GetService<IActivityLogRepository>();
                if (activityLogService != null)
                {
                    try
                    {
                        await activityLogService.LogActivityAsync(
                            userId: null,
                            eventType: ActivityEventType.SystemError,
                            description: $"Unhandled exception in {context.Request.Method} {context.Request.Path}",
                            metadata: new
                            {
                                Exception = exception.GetType().Name,
                                Message = exception.Message,
                                StackTrace = exception.StackTrace,
                                RequestPath = context.Request.Path.ToString(),
                                RequestMethod = context.Request.Method
                            });
                    }
                    catch (Exception logException)
                    {
                        _logger.LogError(logException, "Failed to log activity for unhandled exception");
                    }
                }

                response = new
                {
                    Message = "An internal server error occurred",
                    Details = (string?)null
                };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}