using Domain.Gaming;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class PrizeAwardOptionConfiguration : IEntityTypeConfiguration<PrizeAwardOption>
{
    public void Configure(EntityTypeBuilder<PrizeAwardOption> builder)
    {
        builder.ToTable("prize_award_options", Schemas.Gaming);

        builder.HasKey(option => option.Id);

        builder.Property(option => option.TenantId).IsRequired();
        builder.Property(option => option.PrizeAwardId).IsRequired();
        builder.Property(option => option.PrizeId).IsRequired();
        builder.Property(option => option.PrizeNameSnapshot).HasMaxLength(128).IsRequired();
        builder.Property(option => option.PrizeCostSnapshot).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(option => option.CreatedAt).IsRequired();

        builder.HasIndex(option => new { option.TenantId, option.PrizeAwardId, option.PrizeId })
            .IsUnique();
        // 中文註解：避免同一 Award 出現重複兌獎選項。
    }
}
