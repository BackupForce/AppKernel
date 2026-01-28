using Domain.Gaming.Catalog;
using Domain.Gaming.DrawTemplates;
using Domain.Gaming.Rules;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class DrawTemplatePrizeTierConfiguration : IEntityTypeConfiguration<DrawTemplatePrizeTier>
{
    public void Configure(EntityTypeBuilder<DrawTemplatePrizeTier> builder)
    {
        builder.ToTable("draw_template_prize_tiers", Schemas.Gaming);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.TenantId).IsRequired();
        builder.Property(item => item.TemplateId).IsRequired();
        builder.Property(item => item.PlayTypeCode)
            .HasConversion(code => code.Value, value => new PlayTypeCode(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(item => item.Tier)
            .HasConversion(tier => tier.Value, value => new PrizeTier(value))
            .HasMaxLength(32)
            .IsRequired();

        builder.OwnsOne(
            item => item.Option,
            option =>
            {
                option.WithOwner();
                option.Property(o => o.PrizeId).HasColumnName("prize_id_snapshot");
                option.Property(o => o.Name).HasColumnName("prize_name_snapshot").HasMaxLength(128).IsRequired();
                option.Property(o => o.Cost).HasColumnName("prize_cost_snapshot").HasColumnType("decimal(18,2)").IsRequired();
                option.Property(o => o.RedeemValidDays).HasColumnName("prize_redeem_valid_days_snapshot");
                option.Property(o => o.Description).HasColumnName("prize_description_snapshot").HasMaxLength(256);
            });
        builder.Navigation(item => item.Option).IsRequired();

        builder.HasIndex(item => new { item.TenantId, item.TemplateId, item.PlayTypeCode, item.Tier })
            .IsUnique();
    }
}
