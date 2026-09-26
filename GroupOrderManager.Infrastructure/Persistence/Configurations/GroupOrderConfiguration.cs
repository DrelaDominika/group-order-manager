using GroupOrderManager.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroupOrderManager.Infrastructure.Persistence.Configurations;

public class GroupOrderConfiguration : IEntityTypeConfiguration<GroupOrder>
{
    public void Configure(EntityTypeBuilder<GroupOrder> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Description)
            .HasMaxLength(1000);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}