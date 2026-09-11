using APP.Common.Interfaces;
using Hangfire;
using Infra.BackgroudJobs;
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
        services.AddScoped<IOrderProcessingJob, OrderProcessingJob>();
        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("cs")));

        services.AddHangfireServer();

        // Redis Caching setup
        var redisConn = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConn;
            options.InstanceName = "OrderFlow:";
        });
        services.AddScoped<ICacheService, Infra.Caching.RedisCacheService>();
            
        return services;
    }
}