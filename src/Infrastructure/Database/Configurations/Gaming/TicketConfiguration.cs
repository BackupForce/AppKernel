using Domain.Gaming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Gaming_Tickets");

        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.TenantId).IsRequired();
        builder.Property(ticket => ticket.DrawId).IsRequired();
        builder.Property(ticket => ticket.MemberId).IsRequired();
        builder.Property(ticket => ticket.TotalCost).IsRequired();
        builder.Property(ticket => ticket.CreatedAt).IsRequired();

        builder.HasMany(ticket => ticket.Lines)
            .WithOne()
            .HasForeignKey(line => line.TicketId);

        builder.HasIndex(ticket => new { ticket.TenantId, ticket.MemberId, ticket.CreatedAt });
    }
}
