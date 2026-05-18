using Domain.Gaming.Catalog;
using Domain.Gaming.DrawTemplates;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class DrawTemplateConfiguration : IEntityTypeConfiguration<DrawTemplate>
{
    public void Configure(EntityTypeBuilder<DrawTemplate> builder)
    {
        builder.ToTable("draw_templates", Schemas.Gaming);

        builder.HasKey(template => template.Id);

        builder.Property(template => template.TenantId).IsRequired();
        builder.Property(template => template.GameCode)
            .HasConversion(code => code.Value, value => new GameCode(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(template => template.Name)
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(template => template.IsActive).IsRequired();
        builder.Property(template => template.IsLocked).IsRequired();
        builder.Property(template => template.Version).IsRequired();
        builder.Property(template => template.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();
        builder.Property(template => template.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasMany(template => template.PlayTypes)
            .WithOne()
            .HasForeignKey(playType => playType.TemplateId);

        builder.HasMany(template => template.PrizeTiers)
            .WithOne()
            .HasForeignKey(tier => tier.TemplateId);

        builder.HasMany(template => template.AllowedTicketTemplates)
            .WithOne()
            .HasForeignKey(item => item.TemplateId);

        builder.HasIndex(template => new { template.TenantId, template.GameCode, template.Name })
            .IsUnique();
    }
}
