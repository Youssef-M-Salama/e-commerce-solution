using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class OrderController : ControllerBase
    {
        private readonly AdminOrderService _orderService;

        public OrderController(AdminOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET api/admin/orders?page=1&search=&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] string search = "", [FromQuery] int pageSize = 10)
        {
            var response = await _orderService.GetAllAsync(page, search, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        // GET api/admin/orders/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var response = await _orderService.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        // POST api/admin/orders
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            var response = await _orderService.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT api/admin/orders/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateOrderDto dto)
        {
            var response = await _orderService.UpdateAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT api/admin/orders/5/status
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus([FromRoute] int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var response = await _orderService.UpdateStatusAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE api/admin/orders/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var response = await _orderService.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}