using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImage");

            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.ImageUrl)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(pi => pi.IsMain)
                   .HasDefaultValue(false);

            builder.Property(pi=>pi.DisplayOrder)
                   .IsRequired();

            builder.HasIndex(pi => new { pi.ProductId, pi.IsMain })
            .HasFilter ("[IsMain] = 1");

            builder.HasIndex(pi => new {pi.ProductId,pi.DisplayOrder });


            builder.Property(b => b.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(b => b.UpdatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(pi => pi.Product)
                   .WithMany(p => p.ProductImages) 
                   .HasForeignKey(pi => pi.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
