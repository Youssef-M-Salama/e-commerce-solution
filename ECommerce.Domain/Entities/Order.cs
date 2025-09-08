using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string OrderNumber { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }  

        public string PaymentMethod { get; set; }

        public string PaymentStatus { get; set; }

        public string ShippingAddress { get; set; } 

        public string Notes {  get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public ICollection<OrderItem>OrderItems { get; set; }=new List<OrderItem>();

        // user navigatino will added soon

    }
}
