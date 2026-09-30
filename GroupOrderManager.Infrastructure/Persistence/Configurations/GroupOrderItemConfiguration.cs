using GroupOrderManager.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroupOrderManager.Infrastructure.Persistence.Configurations;

public class GroupOrderItemConfiguration : IEntityTypeConfiguration<GroupOrderItem>
{
    public void Configure(EntityTypeBuilder<GroupOrderItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}