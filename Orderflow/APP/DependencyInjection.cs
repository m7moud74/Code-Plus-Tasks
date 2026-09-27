using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using APP.Common.Behaviors;
using APP.Common.Diagnostics;

namespace APP;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddSingleton<OrderMetrics>();

        return services;
    }
}
