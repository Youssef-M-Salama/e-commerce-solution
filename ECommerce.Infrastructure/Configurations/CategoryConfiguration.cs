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
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c=>c.Name).HasMaxLength(255).IsRequired();

            builder.HasIndex(c=>c.Name).IsUnique();

            builder.Property(c=>c.Description).HasMaxLength(1000).IsRequired();

            builder.Property(c => c.ImageUrl).HasMaxLength(500);

            builder.Property(c=>c.CreatedAt).HasDefaultValue("GETDATE()");
            builder.Property(c=>c.UpdatedAt).HasDefaultValue("GETDATE()");


        }
    }
}
