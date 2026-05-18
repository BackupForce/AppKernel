using Domain.Gaming.TicketClaimEvents;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TicketClaimMemberCounterConfiguration : IEntityTypeConfiguration<TicketClaimMemberCounter>
{
    public void Configure(EntityTypeBuilder<TicketClaimMemberCounter> builder)
    {
        builder.ToTable("ticket_claim_member_counters", Schemas.Gaming);

        builder.HasKey(counter => new { counter.EventId, counter.MemberId });
        builder.Ignore(counter => counter.Id);

        builder.Property(counter => counter.EventId).IsRequired();
        builder.Property(counter => counter.MemberId).IsRequired();
        builder.Property(counter => counter.ClaimedCount).IsRequired();
        builder.Property(counter => counter.UpdatedAtUtc).IsRequired();

        builder.HasIndex(counter => new { counter.EventId, counter.MemberId });
    }
}
