using Domain.Gaming.Campaigns;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class CampaignDrawConfiguration : IEntityTypeConfiguration<CampaignDraw>
{
    public void Configure(EntityTypeBuilder<CampaignDraw> builder)
    {
        builder.ToTable("campaign_draws", Schemas.Gaming);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.TenantId).IsRequired();
        builder.Property(item => item.CampaignId).IsRequired();
        builder.Property(item => item.DrawId).IsRequired();
        builder.Property(item => item.CreatedAtUtc).IsRequired();

        builder.HasIndex(item => new { item.CampaignId, item.DrawId }).IsUnique();
        builder.HasIndex(item => new { item.TenantId, item.CampaignId });
    }
}
