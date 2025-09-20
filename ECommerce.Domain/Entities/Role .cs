using Microsoft.AspNetCore.Identity;

namespace ECommerce.Domain.Entities
{
    public class Role : IdentityRole<int>
    {
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}