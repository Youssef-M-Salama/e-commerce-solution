using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Customer.Controllers
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
        [HttpGet("{id:int}/subcategory")]
        public async Task<IActionResult> GetWithSubCategories([FromRoute] int id)
        {
            var response = await _categoryService.GetWithChildrenAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
