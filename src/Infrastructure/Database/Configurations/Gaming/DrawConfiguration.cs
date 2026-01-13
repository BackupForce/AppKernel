using Domain.Gaming;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class DrawConfiguration : IEntityTypeConfiguration<Draw>
{
    public void Configure(EntityTypeBuilder<Draw> builder)
    {
        builder.ToTable("draws", Schemas.Gaming);

        builder.HasKey(draw => draw.Id);

        builder.Property(draw => draw.TenantId).IsRequired();
        builder.Property(draw => draw.SalesOpenAt).IsRequired();
        builder.Property(draw => draw.SalesCloseAt).IsRequired();
        builder.Property(draw => draw.DrawAt).IsRequired();
        builder.Property(draw => draw.Status).IsRequired();
        builder.Property(draw => draw.IsManuallyClosed).IsRequired();
        builder.Property(draw => draw.ManualCloseAt);
        builder.Property(draw => draw.ManualCloseReason).HasMaxLength(256);
        builder.Property(draw => draw.RedeemValidDays);
        builder.Property(draw => draw.WinningNumbersRaw).HasMaxLength(64);
        builder.Property(draw => draw.ServerSeedHash).HasMaxLength(128);
        builder.Property(draw => draw.ServerSeed).HasMaxLength(256);
        builder.Property(draw => draw.Algorithm).HasMaxLength(64);
        builder.Property(draw => draw.DerivedInput).HasMaxLength(128);
        builder.Property(draw => draw.CreatedAt).IsRequired();
        builder.Property(draw => draw.UpdatedAt).IsRequired();

        builder.HasIndex(draw => new { draw.TenantId, draw.Status });
    }
}
