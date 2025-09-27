using ECommerce.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Customer.Controllers
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
    }
}
