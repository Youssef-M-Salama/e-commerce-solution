using ECommerce.Domain.Entities;

namespace ECommerce.API.Admin.Application.DTOs
{
    public class ProductCategoryDto
    {
        public int CategoryId { get; set; }
        public int ProductId { get; set; }
    }
    public class ProductCategoryReadDto
    {
        public int CategoryId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
