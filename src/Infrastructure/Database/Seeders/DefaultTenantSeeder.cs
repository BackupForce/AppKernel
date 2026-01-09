using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Abstractions.Infrastructure;
using Domain.Security;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

        code = code.Trim().ToUpperInvariant();
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

        Tenant? tenant = await _db.Tenants.FirstOrDefaultAsync(existingTenant => existingTenant.Code == code);

        if (tenant is null)
        {
            tenant = Tenant.Create(Guid.NewGuid(), code, name);
            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync();

            _logger.LogInformation("✅ Default tenant created: {Code} - {Name}", code, name);
        }
        else
        {
            _logger.LogInformation("✅ Default tenant already exists: {Code}", code);
        }

        ResourceNode? rootNode = await _db.ResourceNodes
            .FirstOrDefaultAsync(node => node.TenantId == tenant.Id && node.ExternalKey == "root");

        if (rootNode is null)
        {
            rootNode = ResourceNode.Create(name, "root", tenant.Id);
            _db.ResourceNodes.Add(rootNode);
            await _db.SaveChangesAsync();
        }

        await EnsureSampleResourceTreeAsync(tenant.Id, rootNode.Id);
    }

    private async Task EnsureSampleResourceTreeAsync(Guid tenantId, Guid rootNodeId)
    {
        ResourceNode? departmentNode = await _db.ResourceNodes
            .FirstOrDefaultAsync(node => node.TenantId == tenantId && node.ExternalKey == "department");

        if (departmentNode is null)
        {
            departmentNode = ResourceNode.Create("Default Department", "department", tenantId, rootNodeId);
            _db.ResourceNodes.Add(departmentNode);
            await _db.SaveChangesAsync();
        }

        ResourceNode? projectNode = await _db.ResourceNodes
            .FirstOrDefaultAsync(node => node.TenantId == tenantId && node.ExternalKey == "project");

        if (projectNode is null)
        {
            projectNode = ResourceNode.Create("Default Project", "project", tenantId, departmentNode.Id);
            _db.ResourceNodes.Add(projectNode);
            await _db.SaveChangesAsync();
        }
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
