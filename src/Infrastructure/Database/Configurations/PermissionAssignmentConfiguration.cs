using Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class PermissionAssignmentConfiguration : IEntityTypeConfiguration<PermissionAssignment>
{
    public void Configure(EntityTypeBuilder<PermissionAssignment> builder)
    {
        builder.ToTable("permission_assignments", Schemas.Default);

        builder.HasKey(assignment => assignment.Id);

        builder.Property(assignment => assignment.SubjectType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(assignment => assignment.Decision)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(assignment => assignment.SubjectId)
            .IsRequired();

        builder.Property(assignment => assignment.PermissionCode)
            .IsRequired();

        builder.Property(assignment => assignment.TenantId)
            .IsRequired();

        builder.HasIndex(assignment => new { assignment.SubjectType, assignment.SubjectId });
        builder.HasIndex(assignment => assignment.PermissionCode);
        builder.HasIndex(assignment => assignment.NodeId);
        builder.HasIndex(assignment => assignment.TenantId);

        builder.HasOne(assignment => assignment.Node)
            .WithMany()
            .HasForeignKey(assignment => assignment.NodeId);
    }
}
