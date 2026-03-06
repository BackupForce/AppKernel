using Domain.Gaming.TicketClaimEvents;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TicketClaimEventTagRuleConfiguration : IEntityTypeConfiguration<TicketClaimEventTagRule>
{
    public void Configure(EntityTypeBuilder<TicketClaimEventTagRule> builder)
    {
        builder.ToTable("ticket_claim_event_tag_rules", Schemas.Gaming);

        builder.HasKey(rule => rule.Id);

        builder.Property(rule => rule.TenantId).IsRequired();
        builder.Property(rule => rule.EventId).IsRequired();
        builder.Property(rule => rule.TagId).IsRequired();
        builder.Property(rule => rule.CreatedAtUtc).IsRequired();
        builder.Property(rule => rule.CreatedByUserId).IsRequired(false);

        builder.HasIndex(rule => new { rule.TenantId, rule.EventId, rule.TagId })
            .IsUnique()
            .HasDatabaseName("ux_ticket_claim_event_tag_rules_tenant_event_tag");

        builder.HasIndex(rule => new { rule.TenantId, rule.EventId })
            .HasDatabaseName("ix_ticket_claim_event_tag_rules_tenant_event");

        builder.HasIndex(rule => new { rule.TenantId, rule.TagId })
            .HasDatabaseName("ix_ticket_claim_event_tag_rules_tenant_tag");
    }
}
