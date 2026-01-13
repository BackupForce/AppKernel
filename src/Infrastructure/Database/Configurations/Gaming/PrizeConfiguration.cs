using Domain.Gaming;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class PrizeConfiguration : IEntityTypeConfiguration<Prize>
{
    public void Configure(EntityTypeBuilder<Prize> builder)
    {
        builder.ToTable("prizes", Schemas.Gaming);

        builder.HasKey(prize => prize.Id);

        builder.Property(prize => prize.TenantId).IsRequired();
        builder.Property(prize => prize.Name).HasMaxLength(128).IsRequired();
        builder.Property(prize => prize.Description).HasMaxLength(512);
        builder.Property(prize => prize.Cost).HasPrecision(18, 2).IsRequired();
        builder.Property(prize => prize.IsActive).IsRequired();
        builder.Property(prize => prize.CreatedAt).IsRequired();
        builder.Property(prize => prize.UpdatedAt).IsRequired();

        builder.HasIndex(prize => new { prize.TenantId, prize.Name });
    }
}
