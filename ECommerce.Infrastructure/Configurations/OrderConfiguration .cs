using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

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
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .HasDefaultValue(OrderStatus.Pending);

            builder.Property(o => o.PaymentStatus)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .HasDefaultValue(PaymentStatus.Pending);

            builder.Property(o => o.PaymentMethod)
                   .HasConversion<string>()
                   .HasMaxLength(50);


            builder.Property(o => o.ShippingAddress)
                   .IsRequired();

            builder.Property(o => o.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(o => o.UpdatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.HasMany(o=>o.OrderItems)
                .WithOne(o => o.Order)
                .HasForeignKey(o => o.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(o => o.User)
                   .WithMany(u => u.Orders)
                   .HasForeignKey(o => o.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
