using Domain.Gaming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TicketLineConfiguration : IEntityTypeConfiguration<TicketLine>
{
    public void Configure(EntityTypeBuilder<TicketLine> builder)
    {
        builder.ToTable("Gaming_TicketLines");

        builder.HasKey(line => line.Id);

        builder.Property(line => line.TicketId).IsRequired();
        builder.Property(line => line.LineIndex).IsRequired();
        builder.Property(line => line.NumbersRaw).HasMaxLength(64).IsRequired();

        builder.HasIndex(line => new { line.TicketId, line.LineIndex }).IsUnique();
    }
}
