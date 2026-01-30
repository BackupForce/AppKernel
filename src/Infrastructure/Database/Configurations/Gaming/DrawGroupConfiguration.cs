using Domain.Gaming.DrawGroups;
using Domain.Gaming.Catalog;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class DrawGroupConfiguration : IEntityTypeConfiguration<DrawGroup>
{
    public void Configure(EntityTypeBuilder<DrawGroup> builder)
    {
        builder.ToTable("campaigns", Schemas.Gaming);

        builder.HasKey(drawGroup => drawGroup.Id);

        builder.Property(drawGroup => drawGroup.TenantId).IsRequired();
        builder.Property(drawGroup => drawGroup.GameCode)
            .HasConversion(code => code.Value, value => new GameCode(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(drawGroup => drawGroup.PlayTypeCode)
            .HasConversion(code => code.Value, value => new PlayTypeCode(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(drawGroup => drawGroup.Name).HasMaxLength(128).IsRequired();
        builder.Property(drawGroup => drawGroup.GrantOpenAtUtc).IsRequired();
        builder.Property(drawGroup => drawGroup.GrantCloseAtUtc).IsRequired();
        builder.Property(drawGroup => drawGroup.Status).IsRequired();
        builder.Property(drawGroup => drawGroup.CreatedAtUtc).IsRequired();

        builder.HasMany(c => c.Draws)
            .WithOne()
            .HasForeignKey(d => d.DrawGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Draws)
            .HasField("_draws")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(drawGroup => new { drawGroup.TenantId, drawGroup.GameCode, drawGroup.PlayTypeCode, drawGroup.GrantOpenAtUtc });
    }
}
