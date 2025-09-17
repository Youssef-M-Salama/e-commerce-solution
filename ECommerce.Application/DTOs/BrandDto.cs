using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Admin.Application.DTOs
{

    public class BrandDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public IFormFile? ImageFile { get; set; }
    }


    public class BrandReadDto
    {
        public int Id { get; set; }          
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; } 
    }
}
