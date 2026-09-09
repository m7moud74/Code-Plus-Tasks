using APP.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class InfrastructureService
{
   public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbcontext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("cs")));

        services.AddScoped<IAppDbContext, AppDbcontext>();
            
        return services;
    }
}