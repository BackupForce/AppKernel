using Domain.Gaming;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Gaming;

internal sealed class PrizeAwardConfiguration : IEntityTypeConfiguration<PrizeAward>
{
    public void Configure(EntityTypeBuilder<PrizeAward> builder)
    {
        builder.ToTable("prize_awards", Schemas.Gaming);

        builder.HasKey(award => award.Id);

        builder.Property(award => award.TenantId).IsRequired();
        builder.Property(award => award.MemberId).IsRequired();
        builder.Property(award => award.DrawId).IsRequired();
        builder.Property(award => award.TicketId).IsRequired();
        builder.Property(award => award.LineIndex).IsRequired();
        builder.Property(award => award.MatchedCount).IsRequired();
        builder.Property(award => award.PrizeId).IsRequired();
        builder.Property(award => award.Status).IsRequired();
        builder.Property(award => award.AwardedAt).IsRequired();
        builder.Property(award => award.ExpiresAt);

        builder.HasIndex(award => new { award.TenantId, award.DrawId, award.TicketId, award.LineIndex }).IsUnique();
    }
}
