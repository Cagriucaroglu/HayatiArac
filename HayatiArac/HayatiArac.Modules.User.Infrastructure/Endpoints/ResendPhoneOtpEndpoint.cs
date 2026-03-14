using HayatiArac.Modules.User.Application.Commands.ResendPhoneOtp;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed record ResendPhoneOtpRequest(Guid UserId);

internal sealed class ResendPhoneOtpEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/resend-phone-otp", async (
            ResendPhoneOtpRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ResendPhoneOtpCommand(request.UserId), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { message = "Dogrulama kodu telefonunuza tekrar gonderildi." })
                : Results.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        })
        .WithName("ResendPhoneOtp")
        .WithSummary("Telefon OTP yeniden gönder")
        .WithTags("Authentication")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
