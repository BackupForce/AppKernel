using Domain.Gaming;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class DrawPrizeMappingConfiguration : IEntityTypeConfiguration<DrawPrizeMapping>
{
    public void Configure(EntityTypeBuilder<DrawPrizeMapping> builder)
    {
        builder.ToTable("draw_prize_mappings", Schemas.Gaming);

        builder.HasKey(mapping => mapping.Id);

        builder.Property(mapping => mapping.TenantId).IsRequired();
        builder.Property(mapping => mapping.DrawId).IsRequired();
        builder.Property(mapping => mapping.MatchCount).IsRequired();
        builder.Property(mapping => mapping.PrizeId).IsRequired();
        builder.Property(mapping => mapping.CreatedAt).IsRequired();

        builder.HasIndex(mapping => new { mapping.TenantId, mapping.DrawId, mapping.MatchCount, mapping.PrizeId })
            .IsUnique();
        // 中文註解：避免同一 Draw/MatchCount 重複配置相同獎品。
    }
}
