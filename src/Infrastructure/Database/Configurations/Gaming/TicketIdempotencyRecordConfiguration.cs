using Domain.Gaming.Tickets;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TicketIdempotencyRecordConfiguration : IEntityTypeConfiguration<TicketIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<TicketIdempotencyRecord> builder)
    {
        builder.ToTable("ticket_idempotency_records", Schemas.Gaming);

        builder.HasKey(record => record.Id);

        builder.Property(record => record.TenantId).IsRequired();
        builder.Property(record => record.IdempotencyKey).HasMaxLength(128).IsRequired();
        builder.Property(record => record.Operation).HasMaxLength(64).IsRequired();
        builder.Property(record => record.RequestHash).HasMaxLength(128).IsRequired();
        builder.Property(record => record.ResponsePayload).IsRequired();
        builder.Property(record => record.CreatedAtUtc).IsRequired();

        builder.HasIndex(record => new { record.TenantId, record.IdempotencyKey, record.Operation })
            .IsUnique();
        // 中文註解：索引用於 Idempotency 防重，不建立 migration。
    }
}
