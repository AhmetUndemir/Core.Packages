using Core.CrossCuttingConcerns.Exceptions.Handlers;
using Core.CrossCuttingConcerns.Logging;
using Core.CrossCuttingConcerns.Serilog;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcerns.Exceptions;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpExceptionHandler _httpExceptionHandler;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerServiceBase _loggerService;

    public ExceptionMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor,LoggerServiceBase loggerService)
    {
        _next = next;
        _httpExceptionHandler = new HttpExceptionHandler();
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await LogException(context, exception);
            await HandleExceptionAsync(context.Response, exception);
        }
    }

    private Task LogException(HttpContext context, Exception exception)
    {
        List<LogParameter> parameters = new()
        {
            new LogParameter(){ Value = exception.ToString(), Type = exception.GetType().Name }
        };

        LogDetailWithException logDetail = new()
        {
            ExceptionMessage = exception.Message,
            MethodName = _next.Method.Name,
            Parameters = parameters,
            User = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "Anonymous"
        };

        _loggerService.Error(JsonSerializer.Serialize(logDetail));

        return Task.CompletedTask;
    }

    private Task HandleExceptionAsync(HttpResponse response, Exception exception)
    {
        response.ContentType = "application/json";
        _httpExceptionHandler.Response = response;
        return _httpExceptionHandler.HandleExceptionAsync(exception);
    }
}
