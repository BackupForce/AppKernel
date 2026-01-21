using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.ComplexProperty(
            u => u.Email,
            b => b.Property(e => e.Value).HasColumnName("email"));

        builder.Property(u => u.NormalizedEmail)
            .HasColumnName("normalized_email");

        builder.ComplexProperty(
            u => u.Name,
            b => b.Property(e => e.Value).HasColumnName("name"));

        builder.Property(u => u.Type)
            .HasColumnName("type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.TenantId)
            .HasColumnName("tenant_id");

        builder.HasIndex(u => u.TenantId);

        builder.HasMany(u => u.LoginBindings)
            .WithOne(binding => binding.User)
            .HasForeignKey(binding => binding.UserId);

        builder.ToTable(t => t.HasCheckConstraint(
             "CK_user_type",
             "\"type\" IN (0, 1, 2)"
 ));
    }
}
