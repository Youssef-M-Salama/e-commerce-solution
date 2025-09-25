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

            builder.Property(w => w.UserId).IsRequired();

            builder.HasIndex(w=>w.UserId)
                   .IsUnique();

            builder.Property(w=>w.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(w => w.User)
                .WithOne(u => u.Wishlist)
                     .HasForeignKey<Wishlist>(w => w.UserId)
                     .OnDelete(DeleteBehavior.Cascade); 

        }
    }
}