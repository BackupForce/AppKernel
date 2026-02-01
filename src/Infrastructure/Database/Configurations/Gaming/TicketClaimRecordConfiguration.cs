using Domain.Gaming.TicketClaimEvents;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TicketClaimRecordConfiguration : IEntityTypeConfiguration<TicketClaimRecord>
{
    public void Configure(EntityTypeBuilder<TicketClaimRecord> builder)
    {
        builder.ToTable("ticket_claim_records", Schemas.Gaming);

        builder.HasKey(record => record.Id);

        builder.Property(record => record.TenantId).IsRequired();
        builder.Property(record => record.EventId).IsRequired();
        builder.Property(record => record.MemberId).IsRequired();
        builder.Property(record => record.Quantity).IsRequired();
        builder.Property(record => record.IdempotencyKey).HasMaxLength(64);
        builder.Property(record => record.IssuedTicketIds).HasColumnType("jsonb");
        builder.Property(record => record.ClaimedAtUtc).IsRequired();

        builder.HasIndex(record => new { record.TenantId, record.EventId, record.ClaimedAtUtc });
        builder.HasIndex(record => new { record.TenantId, record.EventId, record.MemberId, record.IdempotencyKey })
            .IsUnique();
    }
}
