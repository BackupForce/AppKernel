using Domain.Gaming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class PrizeRuleConfiguration : IEntityTypeConfiguration<PrizeRule>
{
    public void Configure(EntityTypeBuilder<PrizeRule> builder)
    {
        builder.ToTable("Gaming_PrizeRules");

        builder.HasKey(rule => rule.Id);

        builder.Property(rule => rule.TenantId).IsRequired();
        builder.Property(rule => rule.GameType).IsRequired();
        builder.Property(rule => rule.MatchCount).IsRequired();
        builder.Property(rule => rule.PrizeId).IsRequired();
        builder.Property(rule => rule.IsActive).IsRequired();
        builder.Property(rule => rule.EffectiveFrom);
        builder.Property(rule => rule.EffectiveTo);
        builder.Property(rule => rule.RedeemValidDays);
        builder.Property(rule => rule.CreatedAt).IsRequired();
        builder.Property(rule => rule.UpdatedAt).IsRequired();

        builder.HasIndex(rule => new { rule.TenantId, rule.GameType, rule.MatchCount, rule.IsActive });
        // 中文註解：同一 Tenant/GameType/MatchCount 僅允許一條啟用規則，實際唯一性由程式邏輯保證。
    }
}
