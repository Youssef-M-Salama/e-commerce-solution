using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class UserController : ControllerBase
    {
        private readonly AdminUserService _userService;

        public UserController(AdminUserService userService)
        {
            _userService = userService;
        }

        // GET api/admin/users?page=1&search=&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] string search = "", [FromQuery] int pageSize = 10)
        {
            var response = await _userService.GetAllAsync(page, search, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        // GET api/admin/users/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var response = await _userService.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        // POST api/admin/users
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            var response = await _userService.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT api/admin/users/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUserDto dto)
        {
            var response = await _userService.UpdateAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE api/admin/users/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var response = await _userService.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        // PUT api/admin/users/5/role
        [HttpPut("{id:int}/role")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AssignRole([FromRoute] int id, [FromBody] AssignRoleDto dto)
        {
            var response = await _userService.AssignRoleAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _userService.LoginAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

    }
}