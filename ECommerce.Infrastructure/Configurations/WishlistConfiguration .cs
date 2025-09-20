using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations
{
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            builder.ToTable("Wishlist");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(w => w.Product)
                   .WithMany(p => p.Wishlists) 
                   .HasForeignKey(w => w.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                     .HasForeignKey(w => w.UserId)
                     .OnDelete(DeleteBehavior.Cascade); 

        }
    }
}
