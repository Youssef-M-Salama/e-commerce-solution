using ECommerce.API.Admin.Application.DTOs;
using ECommerce.Customer.API.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Customer.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly CustomerOrderService _orderService;

        public OrderController(CustomerOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET api/orders/user?page=1&pageSize=10
        [HttpGet("Customer")]
        public async Task<IActionResult> GetUserOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var response = await _orderService.GetUserOrdersAsync(userId, page, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        // GET api/orders/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var userId = GetCurrentUserId();
            var response = await _orderService.GetOrderByIdAsync(id, userId);
            return StatusCode(response.StatusCode, response);
        }

        // POST api/orders/cash
        [HttpPost("cash")]
        public async Task<IActionResult> CreateCashOrder([FromBody] CreateCashOrderDto dto)
        {
            var userId = GetCurrentUserId();
            var response = await _orderService.CreateCashOrderAsync(userId, dto);
            return StatusCode(response.StatusCode, response);
        }

       
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst("uid")?.Value ?? "0");
        }
    }
}