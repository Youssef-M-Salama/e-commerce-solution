using ECommerce.API.Admin.Application.DTOs;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.API.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        // POST api/product
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductDto dto)
        {
            var response = await _productService.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT api/product/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromForm] ProductDto dto)
        {
            var response = await _productService.UpdateAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE api/product/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var response = await _productService.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
