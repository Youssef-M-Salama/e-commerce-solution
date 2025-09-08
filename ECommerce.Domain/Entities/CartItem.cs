using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; set; } 

        public int CartId { get; set; } 

        public int ProductId { get; set; }  

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Cart Cart { get; set; }

        public Product Product { get; set; }
    }
}
