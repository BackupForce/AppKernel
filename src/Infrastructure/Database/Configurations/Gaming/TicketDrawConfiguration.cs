using Domain.Gaming.Tickets;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TicketDrawConfiguration : IEntityTypeConfiguration<TicketDraw>
{
    public void Configure(EntityTypeBuilder<TicketDraw> builder)
    {
        builder.ToTable("ticket_draws", Schemas.Gaming);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.TenantId).IsRequired();
        builder.Property(item => item.TicketId).IsRequired();
        builder.Property(item => item.DrawId).IsRequired();
        builder.Property(item => item.ParticipationStatus).IsRequired();
        builder.Property(item => item.CreatedAtUtc).IsRequired();

        builder.HasIndex(item => new { item.TicketId, item.DrawId }).IsUnique();
        builder.HasIndex(item => new { item.TenantId, item.DrawId, item.ParticipationStatus });
    }
}
