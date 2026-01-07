using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Abstractions.Infrastructure;
using Domain.Security;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.Identity;

namespace Infrastructure.Database.Seeders;

public sealed class DefaultTenantSeeder : IDataSeeder
{
    private const string FallbackTenantCode = "DEF";
    private const string FallbackTenantName = "Default Tenant";

    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<DefaultTenantSeeder> _logger;

    public DefaultTenantSeeder(
        ApplicationDbContext db,
        IConfiguration config,
        ILogger<DefaultTenantSeeder> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        string code = _config["DefaultTenant:Code"] ?? FallbackTenantCode;
        string name = _config["DefaultTenant:Name"] ?? FallbackTenantName;

        code = code.Trim();
        name = name.Trim();

        if (!IsValidTenantCode(code))
        {
            _logger.LogWarning("Default tenant code invalid: {Code}", code);
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Default tenant name is empty; skipping seeding.");
            return;
        }

        ResourceNode? tenantNode = await _db.ResourceNodes
            .FirstOrDefaultAsync(node => node.ExternalKey == code);

        if (tenantNode is null)
        {
            tenantNode = ResourceNode.Create(name, code);
            _db.ResourceNodes.Add(tenantNode);
            await _db.SaveChangesAsync();

            _logger.LogInformation("✅ Default tenant created: {Code} - {Name}", code, name);
        }
        else
        {
            _logger.LogInformation("✅ Default tenant already exists: {Code}", code);
        }

        await EnsureRootUserTenantAsync(tenantNode.Id);
    }

    private async Task EnsureRootUserTenantAsync(Guid tenantId)
    {
        string email = _config["RootUser:Email"] ?? RootUser.DefaultEmail;
        Result<Email> emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            _logger.LogWarning("Root user email invalid: {Email}", email);
            return;
        }

        Email rootEmail = emailResult.Value;

        User? rootUser = await _db.Users
            .Include(user => user.UserTenants)
            .FirstOrDefaultAsync(user => user.Email == rootEmail);

        if (rootUser is null)
        {
            _logger.LogWarning("Root user not found for tenant binding: {Email}", email);
            return;
        }

        if (rootUser.HasTenant(tenantId))
        {
            _logger.LogInformation("✅ Root user already bound to default tenant: {Email}", email);
            return;
        }

        rootUser.AssignTenant(tenantId);
        await _db.SaveChangesAsync();

        _logger.LogInformation("✅ Root user bound to default tenant: {Email}", email);
    }

    private static bool IsValidTenantCode(string tenantCode)
    {
        if (string.IsNullOrWhiteSpace(tenantCode))
        {
            return false;
        }

        if (tenantCode.Length != 3)
        {
            return false;
        }

        return tenantCode.All(char.IsLetterOrDigit);
    }
}
