using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities
{
    public class Cart
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        
        public DateTime CreatedAt {  get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public ICollection<CartItem>CartItems { get; set; } = new List<CartItem>();
    }
}
