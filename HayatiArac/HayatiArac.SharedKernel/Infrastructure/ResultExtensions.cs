using HayatiArac.SharedKernel.Application;
using Microsoft.AspNetCore.Http;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace HayatiArac.SharedKernel.Infrastructure;

public static class ResultExtensions
{
    public static HttpResult ToProblem(this Application.IResult result) => result.Error.ToProblem();

    public static HttpResult ToProblem(this Error error)
    {
        var (statusCode, title) = error.Type switch
        {
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Kayıt Bulunamadı"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Çakışma"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Yetkisiz Erişim"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Erişim Reddedildi"),
            _ => (StatusCodes.Status400BadRequest, "Geçersiz İstek")
        };

        return Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: error.Description,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
