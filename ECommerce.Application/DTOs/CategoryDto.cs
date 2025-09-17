using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Admin.Application.DTOs
{
    public class CategoryDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? ParentCategoryId { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

    // Read-only DTO for returning data with hierarchy
    public class CategoryReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }

        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IEnumerable<CategoryChildDto> SubCategories { get; set; } = new List<CategoryChildDto>();
    }

    public class CategoryChildDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
