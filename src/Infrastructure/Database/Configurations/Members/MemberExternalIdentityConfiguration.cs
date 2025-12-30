using Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Members;

internal sealed class MemberExternalIdentityConfiguration : IEntityTypeConfiguration<MemberExternalIdentity>
{
    public void Configure(EntityTypeBuilder<MemberExternalIdentity> builder)
    {
        builder.ToTable("member_external_identities");

        builder.HasKey(identity => identity.Id).HasName("pk_member_external_identities");

        builder.Property(identity => identity.Provider).IsRequired();
        builder.Property(identity => identity.ExternalUserId).IsRequired();
        builder.Property(identity => identity.ExternalUserName).IsRequired();
        builder.Property(identity => identity.CreatedAt).IsRequired();
        builder.Property(identity => identity.UpdatedAt).IsRequired();

        builder
            .HasIndex(identity => new { identity.Provider, identity.ExternalUserId })
            .IsUnique()
            .HasDatabaseName("ix_member_external_identities_provider_external_user_id");

        builder
            .HasIndex(identity => identity.MemberId)
            .HasDatabaseName("ix_member_external_identities_member_id");

        builder
            .HasOne<Member>()
            .WithMany()
            .HasForeignKey(identity => identity.MemberId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_member_external_identities_members_member_id");
    }
}
