using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HayatiArac.SharedKernel.Infrastructure.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (ValidationException ex)
        {
            logger.LogInformation("Doğrulama hatası: {Errors}", ex.Errors.Select(e => e.ErrorMessage));
            await WriteValidationProblemAsync(httpContext, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "İşlenmeyen hata: {Message}", ex.Message);
            await WriteProblemAsync(httpContext, ex);
        }
    }

    private static async Task WriteValidationProblemAsync(HttpContext context, ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => ToFieldName(e.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Geçersiz İstek",
            Detail = "Bir veya daha fazla alan geçersiz.",
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        await WriteAsync(context, problem.Status.Value, problem);
    }

    // "Request.Title" -> "title", "Request.ImageUrls[0]" -> "imageUrls[0]": istemci formundaki alan adıyla eşleşir.
    private static string ToFieldName(string propertyName)
    {
        var lastSegment = propertyName[(propertyName.LastIndexOf('.') + 1)..];
        return JsonNamingPolicy.CamelCase.ConvertName(lastSegment);
    }

    private static async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Geçersiz İstek"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Yetkisiz Erişim"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Kayıt Bulunamadı"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "İşlem Gerçekleştirilemedi"),
            _ => (StatusCodes.Status500InternalServerError, "Sunucu Hatası")
        };

        // 500'lerde iç hata mesajı istemciye sızdırılmaz.
        var detail = statusCode == StatusCodes.Status500InternalServerError
            ? "Beklenmeyen bir hata oluştu."
            : exception.Message;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        await WriteAsync(context, statusCode, problem);
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, ProblemDetails problem)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize<object>(problem, jsonOptions));
    }
}
