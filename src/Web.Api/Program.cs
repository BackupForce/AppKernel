using System.Reflection;
using Application;
using Application.Abstractions.Infrastructure;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Hangfire;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.OpenTelemetry;
using Infrastructure.Database.Seeders;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;
using Web.Api.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.OpenTelemetry(o =>
        {
            o.Endpoint = context.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]!;
            o.ResourceAttributes = new Dictionary<string, object>
            {
                { "service.name", DiagnosticsConfig.ServiceName }
            };
        }));

builder.Services
    .AddApplication()
    .AddPresentation(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
builder.Services.Configure<SuperAdminSeedOptions>(options => options.Enabled = builder.Environment.IsDevelopment());

WebApplication app = builder.Build();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app
    .MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

app.MapEndpoints(versionedGroup);

app.UseBackgroundJobs();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();

    app.UseHangfireDashboard(options: new DashboardOptions
    {
        Authorization = [],
        DarkModeEnabled = false
    });

    app.ApplyMigrations();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseCors(Web.Api.Common.CorsPolicyNames.Default);

app.UseTenantResolution();

app.UseAuthentication();

app.UseAuthorization();

// REMARK: If you want to use Controllers, you'll need this.
app.MapControllers();

using (IServiceScope scope = app.Services.CreateScope())
{
    IEnumerable<IDataSeeder> seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
    foreach (IDataSeeder seeder in seeders)
    {
        await seeder.SeedAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    using IServiceScope scope = app.Services.CreateScope();
    IOptions<SuperAdminSeedOptions> seedOptions =
        scope.ServiceProvider.GetRequiredService<IOptions<SuperAdminSeedOptions>>();
    if (seedOptions.Value.Enabled)
    {
        SuperAdminSeeder superAdminSeeder = scope.ServiceProvider.GetRequiredService<SuperAdminSeeder>();
        await superAdminSeeder.SeedAsync(app.Lifetime.ApplicationStopping);
    }
}

await app.RunAsync();

// REMARK: Required for functional and integration tests to work.
public partial class Program;
