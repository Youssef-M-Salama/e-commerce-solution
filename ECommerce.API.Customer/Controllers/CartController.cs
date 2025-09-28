using ECommerce.API.Admin.Application.Services;
using ECommerce.Application.DTOs.Cart;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }

        // -------------------- GET USER CART ITEMS --------------------
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserCart(int userId)
        {
            var response = await _cartService.GetUserCartAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- ADD ITEM --------------------
        [HttpPost("{userId}/items")]
        public async Task<IActionResult> AddItem(int userId, [FromBody] CartItemCreateDto dto)
        {
            var response = await _cartService.AddItemAsync(userId, dto);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- UPDATE QUANTITY --------------------
        [HttpPut("{userId}/items")]
        public async Task<IActionResult> UpdateItemQuantity(int userId, [FromBody] CartItemUpdateDto dto)
        {
            var response = await _cartService.UpdateQuantityAsync(userId, dto);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- REMOVE ITEM --------------------
        [HttpDelete("{userId}/items/{productId}")]
        public async Task<IActionResult> RemoveItem(int userId, int productId)
        {
            var response = await _cartService.RemoveItemAsync(userId, productId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
