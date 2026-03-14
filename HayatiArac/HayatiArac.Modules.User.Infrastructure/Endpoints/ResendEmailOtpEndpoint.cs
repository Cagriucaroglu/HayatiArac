using HayatiArac.Modules.User.Application.Commands.ResendEmailOtp;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed record ResendEmailOtpRequest(Guid UserId);

internal sealed class ResendEmailOtpEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/resend-email-otp", async (
            ResendEmailOtpRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ResendEmailOtpCommand(request.UserId), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { message = "Dogrulama kodu e-posta adresinize tekrar gonderildi." })
                : Results.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        })
        .WithName("ResendEmailOtp")
        .WithSummary("E-posta OTP yeniden gönder")
        .WithTags("Authentication")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
