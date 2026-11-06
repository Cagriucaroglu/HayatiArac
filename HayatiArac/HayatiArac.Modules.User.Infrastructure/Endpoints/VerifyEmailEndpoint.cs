using HayatiArac.Modules.User.Application.Commands.VerifyEmail;
using HayatiArac.SharedKernel.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HayatiArac.Modules.User.Infrastructure.Endpoints;

internal sealed record VerifyEmailRequest(Guid UserId, string OtpCode);

internal sealed class VerifyEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/verify-email", async (
            VerifyEmailRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new VerifyEmailCommand(request.UserId, request.OtpCode), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { message = "E-posta basariyla dogrulandi." })
                : result.ToProblem();
        })
        .WithName("VerifyEmail")
        .WithSummary("E-posta OTP doğrulama")
        .WithTags("Authentication")
        .AllowAnonymous()
        .RequireRateLimiting(RateLimitPolicies.Otp)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
