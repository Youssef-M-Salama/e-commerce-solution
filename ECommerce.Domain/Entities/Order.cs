using ECommerce.Domain.Enums;
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

        public OrderStatus Status { get; set; }  =OrderStatus.Pending;

        public PaymentMethod PaymentMethod { get; set; } 

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public string ShippingAddress { get; set; } 

        public string Notes {  get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public ICollection<OrderItem>OrderItems { get; set; }=new List<OrderItem>();
        
        public User User { get; set; }

    }
}
