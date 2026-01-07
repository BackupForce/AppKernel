using System.Data;
using Application.Abstractions.Data;
using Domain.Members;
using Domain.Security;
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
    public DbSet<PermissionAssignment> PermissionAssignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);
    }

    public async Task<IDbTransaction> BeginTransactionAsync()
    {
        return (await Database.BeginTransactionAsync()).GetDbTransaction();
    }
}
