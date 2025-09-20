using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Configurations
{
    public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
    {
        public void Configure(EntityTypeBuilder<UserAddress> builder)
        {
            builder.HasKey(ua => ua.Id);
            builder.Property(ua => ua.UserId)
                   .IsRequired();
            builder.Property(ua => ua.Name)
                    .IsRequired()
                    .HasMaxLength(255);
            builder.Property(ua => ua.Street)
                .IsRequired()
                .HasMaxLength(500);
            builder.Property(ua => ua.City)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(ua => ua.State)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(ua => ua.PostalCode)
                .IsRequired()
                .HasMaxLength(20);
            builder.Property(ua => ua.Country)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(ua => ua.IsDefault)
                .HasDefaultValue(false);
            builder.Property(ua => ua.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(ua => ua.User)
                   .WithMany(u => u.UserAddresses)
                   .HasForeignKey(ua => ua.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
