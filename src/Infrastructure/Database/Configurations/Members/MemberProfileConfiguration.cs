using Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Members;

internal sealed class MemberProfileConfiguration : IEntityTypeConfiguration<MemberProfile>
{
    public void Configure(EntityTypeBuilder<MemberProfile> builder)
    {
        builder.ToTable("member_profiles", Schemas.Default);

        builder.HasKey(profile => profile.MemberId);
        builder.Ignore(profile => profile.Id);

        builder.Property(profile => profile.MemberId)
            .HasColumnName("member_id")
            .IsRequired();

        builder.Property(profile => profile.RealName)
            .HasColumnName("real_name")
            .HasMaxLength(64);

        builder.Property(profile => profile.Gender)
            .HasColumnName("gender")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(profile => profile.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(32);

        builder.Property(profile => profile.PhoneVerified)
            .HasColumnName("phone_verified")
            .IsRequired();

        builder.Property(profile => profile.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberProfile>(profile => profile.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
