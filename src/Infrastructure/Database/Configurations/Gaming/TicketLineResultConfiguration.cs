using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TicketLineResultConfiguration : IEntityTypeConfiguration<TicketLineResult>
{
    public void Configure(EntityTypeBuilder<TicketLineResult> builder)
    {
        builder.ToTable("ticket_line_results", Schemas.Gaming);

        builder.HasKey(result => result.Id);

        builder.Property(result => result.TenantId).IsRequired();
        builder.Property(result => result.TicketId).IsRequired();
        builder.Property(result => result.DrawId).IsRequired();
        builder.Property(result => result.LineIndex).IsRequired();
        builder.Property(result => result.PrizeTier)
            .HasConversion(tier => tier.Value, value => new PrizeTier(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(result => result.Payout).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(result => result.SettledAtUtc).IsRequired();

        builder.HasIndex(result => new { result.TenantId, result.TicketId, result.DrawId, result.LineIndex }).IsUnique();
    }
}
