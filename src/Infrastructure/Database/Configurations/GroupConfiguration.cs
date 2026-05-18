using Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups", Schemas.Default);

        builder.HasKey(group => group.Id);

        builder.Property(group => group.Name)
            .IsRequired();

        builder.Property(group => group.ExternalKey)
            .IsRequired();
    }
}
