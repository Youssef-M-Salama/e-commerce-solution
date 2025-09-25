using ECommerce.API.Admin.Application.DTOs;
using System;
using System.Collections.Generic;

namespace ECommerce.Application.DTOs
{
    public class WishlistReadDto
    {
        public int WishListId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<WishlistItemReadDto> Items { get; set; } = new();
    }

    public class WishlistCreateDto
    {
        public int UserId { get; set; }
    }

    public class WishlistDeleteDto
    {
        public int UserId { get; set; }
    }

    public class WishlistItemReadDto
    {
        public int WishListItemId { get; set; }
        public ProductReadDto Product { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }

    public class WishlistItemCreateDto
    {
        public int WishlistId { get; set; }
        public int ProductId { get; set; }
    }

    public class WishlistItemDeleteDto
    {
        public int WishlistId { get; set; }
        public int ProductId { get; set; }
    }
}
