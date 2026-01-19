using Domain.Gaming;
using Domain.Gaming.Catalog;
using Domain.Gaming.Tickets;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets", Schemas.Gaming);

        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.TenantId).IsRequired();
        builder.Property(ticket => ticket.DrawId);
        builder.Property(ticket => ticket.GameCode)
            .HasConversion(code => code.Value, value => new GameCode(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(ticket => ticket.PlayTypeCode)
            .HasConversion(code => code.Value, value => new PlayTypeCode(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(ticket => ticket.MemberId).IsRequired();
        builder.Property(ticket => ticket.CampaignId);
        builder.Property(ticket => ticket.TicketTemplateId);
        builder.Property(ticket => ticket.PriceSnapshot).HasColumnType("decimal(18,2)");
        builder.Property(ticket => ticket.TotalCost);
        builder.Property(ticket => ticket.IssuedAtUtc).IsRequired();
        builder.Property(ticket => ticket.IssuedByType).IsRequired();
        builder.Property(ticket => ticket.IssuedByUserId);
        builder.Property(ticket => ticket.IssuedReason).HasMaxLength(256);
        builder.Property(ticket => ticket.SubmissionStatus).IsRequired();
        builder.Property(ticket => ticket.SubmittedAtUtc);
        builder.Property(ticket => ticket.CancelledAtUtc);
        builder.Property(ticket => ticket.CancelledReason).HasMaxLength(256);
        builder.Property(ticket => ticket.CancelledByUserId);
        builder.Property(ticket => ticket.CreatedAt).IsRequired();

        builder.HasMany(ticket => ticket.Lines)
            .WithOne()
            .HasForeignKey(line => line.TicketId);

        builder.HasIndex(ticket => new { ticket.TenantId, ticket.MemberId, ticket.CreatedAt });
        // 中文註解：索引用於加速會員查詢與期數彙總，不建立 migration。
    }
}
