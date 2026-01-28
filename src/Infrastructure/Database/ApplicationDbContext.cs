using System.Data;
using Application.Abstractions.Data;
using Domain.Auth;
using Domain.Gaming.Campaigns;
using Domain.Gaming.DrawTemplates;
using Domain.Gaming.Draws;
using Domain.Gaming.Entitlements;
using Domain.Gaming.PrizeAwards;
using Domain.Gaming.Prizes;
using Domain.Gaming.RedeemRecords;
using Domain.Gaming.Tickets;
using Domain.Gaming.TicketTemplates;
using Domain.Members;
using Domain.Security;
using Domain.Tenants;
using Domain.Users;
using Infrastructure.Gaming;
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
    public DbSet<Draw> Draws { get; set; }
    public DbSet<DrawTemplate> DrawTemplates { get; set; }
    public DbSet<DrawTemplatePlayType> DrawTemplatePlayTypes { get; set; }
    public DbSet<DrawTemplatePrizeTier> DrawTemplatePrizeTiers { get; set; }
    public DbSet<DrawTemplateAllowedTicketTemplate> DrawTemplateAllowedTicketTemplates { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<CampaignDraw> CampaignDraws { get; set; }
    public DbSet<DrawEnabledPlayType> DrawEnabledPlayTypes { get; set; }
    public DbSet<DrawAllowedTicketTemplate> DrawAllowedTicketTemplates { get; set; }
    public DbSet<DrawPrizePoolItem> DrawPrizePoolItems { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketLine> TicketLines { get; set; }
    public DbSet<TicketDraw> TicketDraws { get; set; }
    public DbSet<TicketLineResult> TicketLineResults { get; set; }
    public DbSet<TicketIdempotencyRecord> TicketIdempotencyRecords { get; set; }
    public DbSet<TicketTemplate> TicketTemplates { get; set; }
    public DbSet<Prize> Prizes { get; set; }
    public DbSet<PrizeAward> PrizeAwards { get; set; }
    public DbSet<RedeemRecord> RedeemRecords { get; set; }
    public DbSet<TenantGameEntitlement> TenantGameEntitlements { get; set; }
    public DbSet<TenantPlayEntitlement> TenantPlayEntitlements { get; set; }
    public DbSet<ResourceNode> ResourceNodes { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<PermissionAssignment> PermissionAssignments { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<LoginBinding> LoginBindings { get; set; }
    public DbSet<AuthSession> AuthSessions { get; set; }
    public DbSet<RefreshTokenRecord> RefreshTokenRecords { get; set; }
    public DbSet<DrawSequence> DrawSequences { get; set; }

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
