using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public int? BrandId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }   
        public int Stock { get; set; }
        public bool IsActive { get; set; }   
        public string? MainImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }   
        public DateTime UpdatedAt { get; set; }

        //Navigation properties
        public Brand Brand { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public ICollection<WishlistItem> WishlistItems { get;set; } = new List<WishlistItem>();

        public ICollection<OrderItem> OrderItems { get; set; }=new List<OrderItem>();
    }

}
