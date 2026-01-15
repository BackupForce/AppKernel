using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Events;
using Application.Abstractions.Gaming;
using Application.Abstractions.Identity;
using Application.Abstractions.Infrastructure;
using Application.Abstractions.Tenants;
using Application.Abstractions.Time;
using Dapper;
using Domain.Members;
using Domain.Gaming;
using Domain.Security;
using Domain.Tenants;
using Domain.Users;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.Caching;
using Infrastructure.Data;
using Infrastructure.Database;
using Infrastructure.Database.Seeders;
using Infrastructure.Events;
using Infrastructure.Extensions;
using Infrastructure.Gaming;
using Infrastructure.Identity;
using Infrastructure.OpenTelemetry;
using Infrastructure.Outbox;
using Infrastructure.Repositories;
using Infrastructure.Settings;
using Infrastructure.Tenants;
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SharedKernel;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddCaching(configuration)
            .AddHealthChecks(configuration)
            .AddMessaging()
            .AddBackgroundJobs(configuration)
            .AddTelemetry()
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<Application.Abstractions.Time.ITimeZoneResolver, TimeZoneResolver>();
        services.AddScoped<ITenantTimeZoneProvider, TenantTimeZoneProvider>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IRootUserService, RootUserService>();
        services.AddScoped<IDataSeeder, RootUserSeeder>();
        services.AddScoped<IDataSeeder, DefaultTenantSeeder>();
        services.AddScoped<IDataSeeder, MemberResourceNodeSeeder>();
        services.AddScoped<SuperAdminSeeder>();
        // 中文註解：外部身份驗證由 Infrastructure 實作。
        services.AddScoped<IExternalIdentityVerifier, LineIdentityVerifier>();
        services.AddScoped<ILottery539RngService, Lottery539RngService>();
        services.AddScoped<IServerSeedStore, ServerSeedStore>();
        services.AddScoped<IWalletLedgerService, WalletLedgerService>();
        services.AddHttpClient();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        string? connectionString = configuration.GetConnectionString("Database");
        Ensure.NotNullOrEmpty(connectionString);

        services.AddSingleton<IDbConnectionFactory>(_ =>
            new DbConnectionFactory(new NpgsqlDataSourceBuilder(connectionString).Build()));

        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

        services.AddDbContext<ApplicationDbContext>(
            (sp, options) => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IResourceNodeRepository, ResourceNodeRepository>();
        services.AddScoped<IDrawRepository, DrawRepository>();
        services.AddScoped<IDrawAllowedTicketTemplateRepository, DrawAllowedTicketTemplateRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketTemplateRepository, TicketTemplateRepository>();
        services.AddScoped<IPrizeRepository, PrizeRepository>();
        services.AddScoped<IPrizeAwardRepository, PrizeAwardRepository>();
        services.AddScoped<IRedeemRecordRepository, RedeemRecordRepository>();
        services.AddScoped<ITenantGameEntitlementRepository, TenantGameEntitlementRepository>();
        services.AddScoped<ITenantPlayEntitlementRepository, TenantPlayEntitlementRepository>();

        return services;
    }

    private static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        string redisConnectionString = configuration.GetConnectionString("Cache")!;

        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddSingleton<ICacheService, CacheService>();
        services.AddScoped<IAuthzCacheInvalidator, AuthzCacheInvalidator>();

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!)
            .AddRedis(configuration.GetConnectionString("Cache")!);

        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryMessageQueue>();
        services.AddTransient<IEventBus, EventBus>();
        services.AddHostedService<IntegrationEventProcessorJob>();

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config =>
            config.UsePostgreSqlStorage(
                options => options.UseNpgsqlConnection(configuration.GetConnectionString("Database")!)));

        services.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(1));

        services.AddScoped<IProcessOutboxMessagesJob, ProcessOutboxMessagesJob>();

        return services;
    }

    private static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(DiagnosticsConfig.ServiceName))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                metrics.AddOtlpExporter();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                tracing.AddOtlpExporter();
            });

        return services;
    }

    public static IServiceCollection AddAuthenticationInternal(this IServiceCollection services, IConfiguration configuration)
    {
        JwtSettings jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings section is missing or malformed.");

        services.AddJwtAuthentication(jwtSettings);

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IEntitlementChecker, EntitlementChecker>();
        services.AddScoped<IEntitlementCacheInvalidator, EntitlementCacheInvalidator>();

        services.AddSingleton<IJwtService>(provider =>
            new JwtService(jwtSettings.SecretKey, jwtSettings.ExpireMinutes));


        services.AddOptions<LineIdentityOptions>()
            .BindConfiguration("LineIdentity")
            .Validate(o => o.VerifyAccessTokenEndpoint is not null, "LineIdentity:VerifyAccessTokenEndpoint is required.")
            .Validate(o => o.ProfileEndpoint is not null, "LineIdentity:ProfileEndpoint is required.")
            .Validate(o => o.VerifyIdTokenEndpoint is not null, "LineIdentity:VerifyIdTokenEndpoint is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ChannelId), "LineIdentity:ChannelId is required.")
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // 中文註解：依 UserType 分流，強制不同 API 走不同授權分支。
            options.AddPolicy(AuthorizationPolicyNames.Platform, policy =>
                policy.AddRequirements(new UserTypeRequirement(
                    new[] { UserType.Platform },
                    false)));
            options.AddPolicy(AuthorizationPolicyNames.TenantUser, policy =>
                policy.AddRequirements(new UserTypeRequirement(
                    new[] { UserType.Tenant },
                    true)));
            options.AddPolicy(AuthorizationPolicyNames.Member, policy =>
                policy.AddRequirements(new UserTypeRequirement(
                    new[] { UserType.Member },
                    true)));
            options.AddPolicy(AuthorizationPolicyNames.MemberActive, policy =>
            {
                policy.AddRequirements(new UserTypeRequirement(
                    new[] { UserType.Member },
                    true));
                policy.AddRequirements(new ActiveMemberRequirement());
            });
            options.AddPolicy(AuthorizationPolicyNames.MemberOwner, policy =>
            {
                policy.AddRequirements(new UserTypeRequirement(
                    new[] { UserType.Member },
                    true));
                policy.AddRequirements(new MemberOwnerRequirement());
            });
        });

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, UserTypeAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, ActiveMemberAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, MemberOwnerAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddScoped<IPermissionProvider, PermissionProvider>();
        services.AddScoped<IGrantedPermissionProvider, GrantedPermissionProvider>();
        services.AddScoped<IPermissionEvaluator, PermissionEvaluator>();

        return services;
    }
}
