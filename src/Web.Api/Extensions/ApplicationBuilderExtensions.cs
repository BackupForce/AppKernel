using Asp.Versioning.ApiExplorer;

namespace Web.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    //public static IApplicationBuilder UseSwaggerWithUi(this WebApplication app)
    //{
    //    app.UseSwagger();
    //    app.UseSwaggerUI(options =>
    //    {
    //        IReadOnlyList<ApiVersionDescription> descriptions = app.DescribeApiVersions();

    //        foreach (ApiVersionDescription description in descriptions)
    //        {
    //            string url = $"/swagger/{description.GroupName}/swagger.json";
    //            string name = description.GroupName.ToUpperInvariant();

    //            options.SwaggerEndpoint(url, name);
    //        }
    //    });

    //    return app;
    //}
    public static IApplicationBuilder UseSwaggerWithUi(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            IReadOnlyList<ApiVersionDescription> descriptions = app.DescribeApiVersions();

            foreach (ApiVersionDescription description in descriptions)
            {
                // 這裡假設 GroupName 命名為：admin-v1、frontend-v1
                string groupName = description.GroupName; // e.g. admin-v1
                string url = $"/swagger/{groupName}/swagger.json";

                string groupKey = groupName.ToUpperInvariant(); // 用這個來 switch 判斷
                string label = groupKey switch
                {
                    var g when g.StartsWith("ADMIN", StringComparison.OrdinalIgnoreCase) => $"🔐 Admin API {description.ApiVersion}",
                    var g when g.StartsWith("FRONTEND", StringComparison.OrdinalIgnoreCase) => $"🌐 Frontend API {description.ApiVersion}",
                    _ => $"API {description.ApiVersion}"
                };

                options.SwaggerEndpoint(url, label);
            }

            options.DisplayRequestDuration();
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });

        return app;
    }

}
