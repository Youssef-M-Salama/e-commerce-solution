using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Errors;
using ECommerce.API.Admin.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.API.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // -------------------- GET ALL --------------------
        // Supports paging, searching, and optional filtering in the future
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] string search = "",
            [FromQuery] int pageSize = 10)
        {
            var response = await _categoryService.GetAllAsync(page, search, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- GET BY ID --------------------
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var response = await _categoryService.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- GET WITH CHILDREN --------------------
        // Future feature: fetch category with all subcategories
        [HttpGet("{id:int}/children")]
        public async Task<IActionResult> GetWithChildren([FromRoute] int id)
        {
            var response = await _categoryService.GetWithChildrenAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- CREATE --------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CategoryDto dto)
        {
            var response = await _categoryService.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- UPDATE --------------------
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromForm] CategoryDto dto)
        {
            var response = await _categoryService.UpdateAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- DELETE --------------------
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var response = await _categoryService.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }

    }
}
