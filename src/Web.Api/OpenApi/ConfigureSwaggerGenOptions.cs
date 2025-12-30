using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Web.Api.OpenApi;

public class ConfigureSwaggerGenOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        //foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
        //{
        //    options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        //}

        //foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
        //{
        //    string version = description.ApiVersion.ToString();

        //    // 加入前後台組別
        //    options.SwaggerDoc($"admin-v{version}", new OpenApiInfo
        //    {
        //        Title = $"RunTrackr Admin API v{version}",
        //        Version = version
        //    });

        //    options.SwaggerDoc($"frontend-v{version}", new OpenApiInfo
        //    {
        //        Title = $"RunTrackr Frontend API v{version}",
        //        Version = version
        //    });
        //}

        var registeredGroups = new HashSet<string>();

        foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
        {
            string version = description.ApiVersion.ToString();

            string[] groups = new[]
            {
            $"admin-v{version}",
            $"frontend-v{version}"
        };

            foreach (string group in groups)
            {
                if (!registeredGroups.Contains(group))
                {
                    var info = new OpenApiInfo
                    {
                        Title = $"{group.ToUpperInvariant()} API",
                        Version = version
                    };

                    options.SwaggerDoc(group, info);
                    registeredGroups.Add(group);
                }
            }
        }

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT 認證。輸入格式: Bearer {your token}",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                Array.Empty<string>()
            }
        });
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    //private static OpenApiInfo CreateVersionInfo(ApiVersionDescription apiVersionDescription)
    //{
    //    var openApiInfo = new OpenApiInfo
    //    {
    //        Title = $"RunTrackr.Api v{apiVersionDescription.ApiVersion}",
    //        Version = apiVersionDescription.ApiVersion.ToString()
    //    };

    //    if (apiVersionDescription.IsDeprecated)
    //    {
    //        openApiInfo.Description += " This API version has been deprecated.";
    //    }

    //    return openApiInfo;
    //}
}
