using HayatiArac.Modules.User.Application.Commands.VerifyPhone;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed record VerifyPhoneRequest(Guid UserId, string OtpCode);

internal sealed class VerifyPhoneEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/verify-phone", async (
            VerifyPhoneRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new VerifyPhoneCommand(request.UserId, request.OtpCode), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { message = "Telefon numarasi basariyla dogrulandi." })
                : Results.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        })
        .WithName("VerifyPhone")
        .WithSummary("Telefon OTP doğrulama")
        .WithTags("Authentication")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
