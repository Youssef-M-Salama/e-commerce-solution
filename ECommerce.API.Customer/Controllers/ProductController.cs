using ECommerce.API.Admin.Application.DTOs;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        // GET api/product?page=1&search=&categoryId=1&brandId=2&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] string search = "",
            [FromQuery] int? categoryId = null,
            [FromQuery] int? brandId = null,
            [FromQuery] int pageSize = 10)
        {
            var response = await _productService.GetAllAsync(page, search, categoryId, brandId, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        // GET api/product/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var response = await _productService.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

    }
}
