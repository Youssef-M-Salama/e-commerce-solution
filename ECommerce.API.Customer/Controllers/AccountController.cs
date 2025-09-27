using ECommerce.API.Admin.Application.DTOs;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace ECommerce.API.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly CustomerAuthService _customerAuthService;

        public AccountController(CustomerAuthService customerAuthService)
        {
            _customerAuthService = customerAuthService;
        }

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var response = await _customerAuthService.RegisterAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // ---------------- LOGIN ----------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var response = await _customerAuthService.LoginAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // ---------------- FORGOT PASSWORD ----------------
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var response = await _customerAuthService.ForgotPasswordAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // ---------------- RESET PASSWORD ----------------
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var response = await _customerAuthService.ResetPasswordAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{id:int}/profile")]
        public async Task<IActionResult> GetProfileAsync(int id)
        {
            var response = await _customerAuthService.GetProfileAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id:int}/profile")]
        public async Task<IActionResult> UpdateProfileAsync(int id, [FromBody] UpdateProfileDto dto)
        {
            var response = await _customerAuthService.UpdateProfileAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }
    }
}
