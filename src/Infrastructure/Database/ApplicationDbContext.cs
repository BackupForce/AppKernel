using System.Data;
using Application.Abstractions.Data;
using Domain.Members;
using Domain.Security;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<MemberPointBalance> MemberPointBalances { get; set; }
    public DbSet<MemberPointLedger> MemberPointLedgers { get; set; }
    public DbSet<MemberAssetBalance> MemberAssetBalances { get; set; }
    public DbSet<MemberAssetLedger> MemberAssetLedgers { get; set; }
    public DbSet<MemberActivityLog> MemberActivityLogs { get; set; }
    public DbSet<ResourceNode> ResourceNodes { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<PermissionAssignment> PermissionAssignments { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);
    }

    public async Task<IDbTransaction> BeginTransactionAsync()
    {
        return (await Database.BeginTransactionAsync()).GetDbTransaction();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new UniqueConstraintViolationException("Database update failed due to a conflict.", ex);
        }
    }
}
