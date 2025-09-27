using ECommerce.API.Admin.Application.Services;
using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly WishlistService _wishlistService;

        public WishlistController(WishlistService wishlistService)
        {
            _wishlistService = wishlistService ?? throw new ArgumentNullException(nameof(wishlistService));
        }

        // -------------------- GET WISHLIST BY USER --------------------
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var response = await _wishlistService.GetByUserIdAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- ADD ITEM --------------------
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] WishlistItemCreateDto dto)
        {
            var response = await _wishlistService.AddItemAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- REMOVE ITEM --------------------
        [HttpDelete("items")]
        public async Task<IActionResult> RemoveItem([FromBody] WishlistItemDeleteDto dto)
        {
            var response = await _wishlistService.RemoveItemAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
    }
}
