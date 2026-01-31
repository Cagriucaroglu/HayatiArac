using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HayatiArac.SharedKernel.Infrastructure;

public interface IModuleRegistration
{
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
    void ConfigureMiddleware(WebApplication app);
    void ConfigureEndpoints(WebApplication app);
}
