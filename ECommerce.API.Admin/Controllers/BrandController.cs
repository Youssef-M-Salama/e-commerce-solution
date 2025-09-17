using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.API.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        // POST api/brand
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] BrandDto dto)
        {
            var response = await _brandService.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT api/brand/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromForm] BrandDto dto)
        {
            var response = await _brandService.UpdateAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE api/brand/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var response = await _brandService.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
