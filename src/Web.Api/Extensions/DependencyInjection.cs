using Asp.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Web.Api.Common;
using Web.Api.Infrastructure;
using Web.Api.OpenApi;
using Web.Api.Settings;
using System.Linq;

namespace Web.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // REMARK: If you want to use Controllers, you'll need this.
        services.AddControllers();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        services.ConfigureOptions<ConfigureSwaggerGenOptions>();

        CorsSettings corsSettings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()
            ?? throw new InvalidOperationException("Cors section is missing or malformed.");

        if (corsSettings.AllowedOrigins.Count is 0)
        {
            throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin.");
        }

        if (corsSettings.AllowCredentials && corsSettings.AllowedOrigins.Any(origin =>
                string.Equals(origin, "*", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Cors:AllowedOrigins cannot contain \"*\" when AllowCredentials is true.");
        }

        services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.SectionName));

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyNames.Default, policyBuilder =>
        services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.SectionName));

        services.AddCors(options => options.AddPolicy(CorsPolicyNames.Default, policyBuilder =>
            {
                policyBuilder
                    .WithOrigins([.. corsSettings.AllowedOrigins])
                    .AllowAnyHeader()
                    .AllowAnyMethod();

                if (corsSettings.AllowCredentials)
                {
                    policyBuilder.AllowCredentials();
                }
                else
                {
                    policyBuilder.DisallowCredentials();
                }
            });
        });
            }));

        return services;
    }
}
