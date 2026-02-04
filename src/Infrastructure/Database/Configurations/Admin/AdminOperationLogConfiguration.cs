using Domain.Admin.OperationLogs;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Admin;

internal sealed class AdminOperationLogConfiguration : IEntityTypeConfiguration<AdminOperationLog>
{
    public void Configure(EntityTypeBuilder<AdminOperationLog> builder)
    {
        builder.ToTable("admin_operation_logs", Schemas.Admin);

        builder.HasKey(l => l.Id);

        builder.Property(l => l.TenantId).IsRequired();
        builder.Property(l => l.TargetType).HasMaxLength(64).IsRequired();
        builder.Property(l => l.TargetId).IsRequired();
        builder.Property(l => l.Action).HasMaxLength(64).IsRequired();
        builder.Property(l => l.OperatorUserId);
        builder.Property(l => l.OperatorType).HasMaxLength(32);
        builder.Property(l => l.Reason).HasMaxLength(512);
        builder.Property(l => l.MetadataJson).HasColumnType("jsonb");
        builder.Property(l => l.OccurredAtUtc).IsRequired();
        builder.Property(l => l.CreatedAtUtc).IsRequired();
        builder.Property(l => l.DedupKey).HasMaxLength(128);

        builder.HasIndex(l => new { l.TenantId, l.TargetType, l.TargetId, l.OccurredAtUtc });
        builder.HasIndex(l => new { l.TenantId, l.OperatorUserId, l.OccurredAtUtc });
        builder.HasIndex(l => l.DedupKey).IsUnique();
    }
}
