using ECommerce.Domain.Entities;

namespace ECommerce.Application.DTOs.Cart
{
    public class CartReadDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
    // For reading cart items
    public class CartItemReadDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    // For adding an item
    public class CartItemCreateDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    // For updating quantity
    public class CartItemUpdateDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
