using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                   .HasMaxLength(255)
                   .IsRequired();

            builder.HasIndex(b => b.Name) 
                   .IsUnique();

            builder.Property(b => b.Description)
                   .HasMaxLength(1000);

            builder.Property(b => b.LogoUrl)
                   .HasMaxLength(500)
            .IsRequired(false);

            builder.Property(b => b.IsActive)
                   .HasDefaultValue(true);

            builder.Property(b => b.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(b => b.UpdatedAt)
                   .HasDefaultValueSql("GETDATE()");


            builder.HasMany(b => b.Products)
                   .WithOne(p => p.Brand)
                   .HasForeignKey(p => p.BrandId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
