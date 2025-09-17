using ECommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ECommerce.API.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly ProductCategoryService _productCategoryService;

        public ProductCategoryController(ProductCategoryService productCategoryService)
        {
            _productCategoryService = productCategoryService;
        }


        [HttpGet("{productId:int}")]
        public async Task<IActionResult> GetProductCategoriesAsync([FromRoute] int productId)
        {
            var response = await _productCategoryService.GetProductCategoriesAsync(productId);
            return StatusCode(response.StatusCode, response);
        }


        [HttpGet("{productId:int}/categories/{categoryId:int}")]
        public async Task<IActionResult> GetByProductAndCategoryAsync([FromRoute] int productId, [FromRoute] int categoryId)
        {
            var response = await _productCategoryService.GetByProductAndCategoryAsync(productId, categoryId);
            return StatusCode(response.StatusCode, response);
        }


        [HttpPost("{productId:int}/categories/{categoryId:int}")]
        public async Task<IActionResult> CreateAsync([FromRoute] int productId, [FromRoute] int categoryId)
        {
            var response = await _productCategoryService.CreateAsync(productId, categoryId);
            return StatusCode(response.StatusCode, response);
        }


        [HttpPost("{productId:int}/categories")]
        public async Task<IActionResult> AddMultipleAsync([FromRoute] int productId, [FromBody] List<int> categoryIds)
        {
            var response = await _productCategoryService.AddMultipleAsync(productId, categoryIds);
            return StatusCode(response.StatusCode, response);
        }


        [HttpPut("{productId:int}/categories")]
        public async Task<IActionResult> ReplaceAsync([FromRoute] int productId, [FromBody] List<int> categoryIds)
        {
            var response = await _productCategoryService.ReplaceAsync(productId, categoryIds);
            return StatusCode(response.StatusCode, response);
        }


        [HttpDelete("{productId:int}/categories/{categoryId:int}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int productId, [FromRoute] int categoryId)
        {
            var response = await _productCategoryService.DeleteAsync(productId, categoryId);
            return StatusCode(response.StatusCode, response);
        }


        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> DeleteByProductAsync([FromRoute] int productId)
        {
            var response = await _productCategoryService.DeleteByProductAsync(productId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
