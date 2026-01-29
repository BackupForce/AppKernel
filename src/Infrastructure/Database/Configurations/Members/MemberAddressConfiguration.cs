using Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Members;

internal sealed class MemberAddressConfiguration : IEntityTypeConfiguration<MemberAddress>
{
    public void Configure(EntityTypeBuilder<MemberAddress> builder)
    {
        builder.ToTable("member_addresses", Schemas.Default);

        builder.HasKey(address => address.Id);

        builder.Property(address => address.MemberId)
            .HasColumnName("member_id")
            .IsRequired();

        builder.Property(address => address.ReceiverName)
            .HasColumnName("receiver_name")
            .IsRequired();

        builder.Property(address => address.PhoneNumber)
            .HasColumnName("phone_number")
            .IsRequired();

        builder.Property(address => address.Country)
            .HasColumnName("country")
            .IsRequired();

        builder.Property(address => address.City)
            .HasColumnName("city")
            .IsRequired();

        builder.Property(address => address.District)
            .HasColumnName("district")
            .IsRequired();

        builder.Property(address => address.AddressLine)
            .HasColumnName("address_line")
            .IsRequired();

        builder.Property(address => address.IsDefault)
            .HasColumnName("is_default")
            .IsRequired();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(address => address.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
