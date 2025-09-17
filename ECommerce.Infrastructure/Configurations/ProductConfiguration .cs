using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                   .HasMaxLength(255)
                   .IsRequired();

            builder.HasIndex(p => p.Name); 

            builder.Property(p => p.Description)
                   .HasMaxLength(2000);

            builder.Property(p => p.Price)
                   .HasColumnType("decimal(18,2)") 
                   .IsRequired();

            builder.Property(p => p.Stock)
                   .HasDefaultValue(0);

            builder.Property(p => p.IsActive)
                   .HasDefaultValue(true);

            builder.Property(p => p.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.UpdatedAt)
                   .HasDefaultValueSql("GETDATE()");


            builder.HasOne(p => p.Brand)
                   .WithMany(b => b.Products)
                   .HasForeignKey(p => p.BrandId)
                   .OnDelete(DeleteBehavior.SetNull);

            //builder.HasOne(p => p.SubCategory)
            //       .WithMany(sc => sc.Products)
            //       .HasForeignKey(p => p.SubCategoryId)
            //       .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.ProductImages)
                   .WithOne(pi => pi.Product)
                   .HasForeignKey(pi => pi.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.ProductCategories)
                   .WithOne(pc => pc.Product)
                   .HasForeignKey(pc => pc.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Wishlists)
               .WithOne(w => w.Product)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
