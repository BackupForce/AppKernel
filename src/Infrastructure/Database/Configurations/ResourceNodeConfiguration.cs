using Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class ResourceNodeConfiguration : IEntityTypeConfiguration<ResourceNode>
{
    public void Configure(EntityTypeBuilder<ResourceNode> builder)
    {
        builder.ToTable("resource_nodes", Schemas.Default);

        builder.HasKey(node => node.Id);

        builder.Property(node => node.Name)
            .IsRequired();

        builder.Property(node => node.ExternalKey)
            .IsRequired();

        builder.HasIndex(node => node.ExternalKey)
            .IsUnique();

        builder.HasIndex(node => node.ParentId);

        builder.HasOne(node => node.Parent)
            .WithMany(node => node.Children)
            .HasForeignKey(node => node.ParentId);
    }
}
