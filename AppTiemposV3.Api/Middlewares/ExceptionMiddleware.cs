using System.Net;
using AppTiemposV3.SharedClases.Exceptions;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace AppTiemposV3.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }
    
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }
    
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int statusCode = (int)HttpStatusCode.InternalServerError;
        string message = "Ocurrió un error inesperado.";
        string path = context.Request.HttpContext.Request.Path.Value ?? "/api/error";
        string timestamp = DateTime.Now.ToString("O");

        switch (exception)
        {
            case BadRequestException badRequest:
                statusCode = badRequest.StatusCode;
                message = badRequest.Message!;
                _logger.LogError(exception, badRequest.Message!);
                break;
            case UnauthorizedException unauthorized:
                statusCode = unauthorized.StatusCode;
                message = unauthorized.Message!;
                _logger.LogError(exception, unauthorized.Message!);
                break;
            case ForbiddenException forbidden:
                statusCode = forbidden.StatusCode;
                message = forbidden.Message!;
                _logger.LogError(exception, forbidden.Message!);
                break;
            case NotFoundException notFound:
                statusCode = notFound.StatusCode;
                message = notFound.Message!;
                _logger.LogError(exception, notFound.Message!);
                break;
            case InternalServerErrorException internalErr:
                statusCode = internalErr.StatusCode;
                message = internalErr.Message!;
                _logger.LogError(exception, internalErr.Message!);
                break;
            default:
                _logger.LogError(exception, "Unhandled exception");
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            statusCode,
            message,
            path,
            timestamp,
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}