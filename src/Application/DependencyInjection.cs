using Application.Abstractions.Behaviors;
using Application.Abstractions.Time;
using Application.Time;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            config.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionalPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(QueryCachingPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddSingleton<Authorization.PermissionUiCatalogProvider>();
        services.AddSingleton<Authorization.PermissionSelectionNormalizer>();
        services.AddSingleton<IUtcRangeCalculator, UtcRangeCalculator>();

        return services;
    }
}
