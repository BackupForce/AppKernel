using Domain.Gaming;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class DrawEnabledPlayTypeConfiguration : IEntityTypeConfiguration<DrawEnabledPlayType>
{
    public void Configure(EntityTypeBuilder<DrawEnabledPlayType> builder)
    {
        builder.ToTable("draw_enabled_play_types", Schemas.Gaming);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.TenantId).IsRequired();
        builder.Property(item => item.DrawId).IsRequired();
        builder.Property(item => item.PlayTypeCode)
            .HasConversion(code => code.Value, value => new PlayTypeCode(value))
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(item => new { item.TenantId, item.DrawId, item.PlayTypeCode }).IsUnique();
    }
}
