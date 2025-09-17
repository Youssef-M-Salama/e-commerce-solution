using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Admin.Application.DTOs
{
    public class ProductImageDto
    {
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
        public IFormFile ImageFile { get; set; }

    }
    public class ProductImageReadDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
