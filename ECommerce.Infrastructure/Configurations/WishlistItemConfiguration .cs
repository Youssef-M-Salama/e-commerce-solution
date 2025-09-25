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
    public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
    {
        public void Configure(EntityTypeBuilder<WishlistItem> builder)
        {
            builder.HasKey(wi => wi.Id);

            builder.Property(wi => wi.WishlistId).IsRequired();
            builder.Property(wi=>wi.ProductId).IsRequired();

            builder.Property(wi => wi.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.HasIndex(wi => new { wi.WishlistId, wi.ProductId })
                   .IsUnique();


            builder.HasOne(wi=>wi.Product)
                .WithMany(p=>p.WishlistItems)
                .HasForeignKey(wi=>wi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(wi => wi.Wishlist)
                .WithMany(w=> w.Items)
                .HasForeignKey(wi => wi.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
