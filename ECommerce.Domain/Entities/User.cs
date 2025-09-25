using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities
{
    public class User : IdentityUser<int>
    {
        public bool IsActive { get; set; } = true;
        public string? ResetPasswordCode { get; set; }
        public DateTime? ResetPasswordExpiry { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
        public Wishlist Wishlist { get; set; }
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
