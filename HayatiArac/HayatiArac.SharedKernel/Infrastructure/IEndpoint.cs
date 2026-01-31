using Microsoft.AspNetCore.Routing;

namespace HayatiArac.SharedKernel.Infrastructure;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
