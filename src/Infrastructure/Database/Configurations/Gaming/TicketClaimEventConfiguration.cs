using Domain.Gaming.TicketClaimEvents;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TicketClaimEventConfiguration : IEntityTypeConfiguration<TicketClaimEvent>
{
    public void Configure(EntityTypeBuilder<TicketClaimEvent> builder)
    {
        builder.ToTable("ticket_claim_events", Schemas.Gaming);

        builder.HasKey(ticketClaimEvent => ticketClaimEvent.Id);

        builder.Property(ticketClaimEvent => ticketClaimEvent.TenantId).IsRequired();
        builder.Property(ticketClaimEvent => ticketClaimEvent.Name).HasMaxLength(128).IsRequired();
        builder.Property(ticketClaimEvent => ticketClaimEvent.StartsAtUtc).IsRequired();
        builder.Property(ticketClaimEvent => ticketClaimEvent.EndsAtUtc).IsRequired();
        builder.Property(ticketClaimEvent => ticketClaimEvent.Status).IsRequired();
        builder.Property(ticketClaimEvent => ticketClaimEvent.TotalQuota).IsRequired();
        builder.Property(ticketClaimEvent => ticketClaimEvent.TotalClaimed).IsRequired();
        builder.Property(ticketClaimEvent => ticketClaimEvent.PerMemberQuota).IsRequired();
        builder.Property(ticketClaimEvent => ticketClaimEvent.ScopeType).IsRequired();
        builder.Property(ticketClaimEvent => ticketClaimEvent.ScopeId).IsRequired();
        builder.Property(ticketClaimEvent => ticketClaimEvent.TicketTemplateId);
        builder.Property(ticketClaimEvent => ticketClaimEvent.CreatedAtUtc).IsRequired();
        builder.Property(ticketClaimEvent => ticketClaimEvent.UpdatedAtUtc).IsRequired();

        builder.HasIndex(ticketClaimEvent => new
            { ticketClaimEvent.TenantId, ticketClaimEvent.Status, ticketClaimEvent.StartsAtUtc, ticketClaimEvent.EndsAtUtc });
    }
}
