using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Nodes;
public sealed class NodeRelationConfiguration : IEntityTypeConfiguration<NodeRelation>
{
    public void Configure(EntityTypeBuilder<NodeRelation> builder)
    {
        builder.HasKey(r => new { r.AncestorNodeId, r.DescendantNodeId });

        builder.Property(r => r.Depth)
            .IsRequired();

        builder.Property(r => r.IsDeleted)
            .IsRequired();

        builder.HasOne(r => r.AncestorNode)
            .WithMany(n => n.Descendants)
            .HasForeignKey(r => r.AncestorNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.DescendantNode)
            .WithMany(n => n.Ancestors)
            .HasForeignKey(r => r.DescendantNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // 可選：加上軟刪除過濾
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
