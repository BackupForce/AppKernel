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

        builder.Property(node => node.TenantId)
            .IsRequired();

        builder.HasIndex(node => node.TenantId)
            .HasDatabaseName("ix_resource_nodes_tenant_id");

        builder.HasIndex(node => new { node.TenantId, node.ExternalKey })
            .IsUnique()
            .HasDatabaseName("ux_resource_nodes_tenant_id_external_key");

        builder.HasIndex(node => node.ParentId);

        builder.HasOne(node => node.Parent)
            .WithMany(node => node.Children)
            .HasForeignKey(node => node.ParentId);
    }
}
