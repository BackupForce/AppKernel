using Domain.Gaming.Catalog;
using Domain.Gaming.DrawTemplates;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class DrawTemplatePlayTypeConfiguration : IEntityTypeConfiguration<DrawTemplatePlayType>
{
    public void Configure(EntityTypeBuilder<DrawTemplatePlayType> builder)
    {
        builder.ToTable("draw_template_play_types", Schemas.Gaming);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.TenantId).IsRequired();
        builder.Property(item => item.TemplateId).IsRequired();
        builder.Property(item => item.PlayTypeCode)
            .HasConversion(code => code.Value, value => new PlayTypeCode(value))
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(item => new { item.TenantId, item.TemplateId, item.PlayTypeCode })
            .IsUnique();
    }
}
