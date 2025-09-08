using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations
{
    public class SubCategoryConfiguration : IEntityTypeConfiguration<SubCategory>
    {
        public void Configure(EntityTypeBuilder<SubCategory> builder)
        {
            builder.ToTable("SubCategories");

            builder.HasKey(sc => sc.Id);

            builder.Property(sc => sc.Name)
                   .HasMaxLength(255)
                   .IsRequired();

            builder.HasIndex(sc => new { sc.CategoryId, sc.Name }) 
                   .IsUnique();

            builder.Property(sc => sc.Description)
                   .HasMaxLength(1000);

            builder.Property(sc => sc.ImageUrl)
                   .HasMaxLength(500);

            builder.Property(sc => sc.IsActive)
                   .HasDefaultValue(true);

            builder.Property(sc => sc.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(sc => sc.UpdatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(sc => sc.Category)
                   .WithMany(c => c.SubCategories)
                   .HasForeignKey(sc => sc.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
