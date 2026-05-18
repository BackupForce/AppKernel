using Domain.Gaming.DrawGroups;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class DrawGroupDrawConfiguration : IEntityTypeConfiguration<DrawGroupDraw>
{
    public void Configure(EntityTypeBuilder<DrawGroupDraw> builder)
    {
        builder.ToTable("draw_group_draws", Schemas.Gaming);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.TenantId).IsRequired();
        builder.Property(item => item.DrawGroupId)
            .HasColumnName("draw_group_id")
            .IsRequired();
        builder.Property(item => item.DrawId).IsRequired();
        builder.Property(item => item.CreatedAtUtc).IsRequired();

        builder.HasIndex(item => new { item.TenantId, item.DrawGroupId, item.DrawId }).IsUnique();
        builder.HasIndex(item => new { item.TenantId, item.DrawGroupId });
    }
}
