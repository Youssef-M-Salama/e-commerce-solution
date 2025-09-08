using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Order");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(o => o.OrderNumber)
                   .IsUnique();

            builder.Property(o => o.TotalAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(o => o.Status)
                   .HasMaxLength(50)
                   .HasDefaultValue("Pending");

            builder.Property(o => o.PaymentMethod)
                   .HasMaxLength(50);

            builder.Property(o => o.PaymentStatus)
                   .HasMaxLength(50)
                   .HasDefaultValue("Pending");

            builder.Property(o => o.ShippingAddress)
                   .IsRequired();

            builder.Property(o => o.Notes);

            builder.Property(o => o.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(o => o.UpdatedAt)
                   .HasDefaultValueSql("GETDATE()");\

            builder.HasMany(o=>o.OrderItems)
                .WithOne(o => o.Order)
                .HasForeignKey(o => o.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
