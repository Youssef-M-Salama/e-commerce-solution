using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Customer.Controllers
{
    [ApiController]
    [Route("api/user-addresses")]
    public class UserAddressController : ControllerBase
    {
        private readonly UserAddressService _addressService;

        public UserAddressController(UserAddressService addressService)
        {
            _addressService = addressService;
        }

      
        // GET api/admin/user-addresses/user/3
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUserId([FromRoute] int userId)
        {
            var response = await _addressService.GetByUserAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

        // POST api/admin/user-addresses
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserAddressDto dto)
        {
            var response = await _addressService.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT api/admin/user-addresses/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUserAddressDto dto)
        {
            var response = await _addressService.UpdateAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE api/admin/user-addresses/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var response = await _addressService.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
