using Domain.Gaming.Campaigns;
using Domain.Gaming.Catalog;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("campaigns", Schemas.Gaming);

        builder.HasKey(campaign => campaign.Id);

        builder.Property(campaign => campaign.TenantId).IsRequired();
        builder.Property(campaign => campaign.GameCode)
            .HasConversion(code => code.Value, value => new GameCode(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(campaign => campaign.PlayTypeCode)
            .HasConversion(code => code.Value, value => new PlayTypeCode(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(campaign => campaign.Name).HasMaxLength(128).IsRequired();
        builder.Property(campaign => campaign.GrantOpenAtUtc).IsRequired();
        builder.Property(campaign => campaign.GrantCloseAtUtc).IsRequired();
        builder.Property(campaign => campaign.Status).IsRequired();
        builder.Property(campaign => campaign.CreatedAtUtc).IsRequired();

        builder.HasMany(c => c.Draws)
            .WithOne()
            .HasForeignKey(d => d.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Draws)
            .HasField("_draws")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(campaign => new { campaign.TenantId, campaign.GameCode, campaign.PlayTypeCode, campaign.GrantOpenAtUtc });
    }
}
