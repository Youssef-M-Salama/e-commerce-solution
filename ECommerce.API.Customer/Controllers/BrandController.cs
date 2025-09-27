using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly BrandService _brandService;

        public BrandController(BrandService brandService)
        {
            _brandService = brandService;
        }

        // GET api/brand?page=1&search=&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] string search = "", [FromQuery] int pageSize = 10)
        {
            var response = await _brandService.GetAllAsync(page, search, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        // GET api/brand/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var response = await _brandService.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
