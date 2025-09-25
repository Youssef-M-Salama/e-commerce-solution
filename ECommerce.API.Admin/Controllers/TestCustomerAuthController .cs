using ECommerce.API.Admin.Application.DTOs;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Admin.Controllers
{
    [ApiController]
    [Route("api/test/customers/auth")]
    [AllowAnonymous] // ✅ Allow access without Admin role
    public class TestCustomerAuthController : ControllerBase
    {
        private readonly CustomerAuthService _authService;

        public TestCustomerAuthController(CustomerAuthService authService)
        {
            _authService = authService;
        }

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var response = await _authService.RegisterAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // ---------------- LOGIN ----------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var response = await _authService.LoginAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // ---------------- FORGOT PASSWORD ----------------
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var response = await _authService.ForgotPasswordAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // ---------------- RESET PASSWORD ----------------
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var response = await _authService.ResetPasswordAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{id:int}/profile")]
        public async Task<IActionResult> GetProfileAsync(int id)
        {
            var response = await _authService.GetProfileAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id:int}/profile")]
        public async Task<IActionResult>UpdateProfileAsync(int id, [FromBody] UpdateProfileDto dto)
        {
            var response = await _authService.UpdateProfileAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }
    }
}
