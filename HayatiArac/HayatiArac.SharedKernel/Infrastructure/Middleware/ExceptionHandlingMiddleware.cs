using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace HayatiArac.SharedKernel.Infrastructure.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task TaskAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "İşlenmeyen hata: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }

    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest , "Geçersiz İstek"),
            ArgumentException => (HttpStatusCode.BadRequest , "Geçersiz İstek"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized , "Yetkisiz Erişim"),
            KeyNotFoundException => (HttpStatusCode.NotFound , "Kayıt bulunamadı"),
            InvalidOperationException => (HttpStatusCode.Conflict, "İşlem Gerçekleştirilemedi"),
            _ => (HttpStatusCode.InternalServerError, "Sunucu Hatası")
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = statusCode,
            title,
            detail = exception.Message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
