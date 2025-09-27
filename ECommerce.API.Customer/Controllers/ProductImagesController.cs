using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Customer.Controllers
{
    [ApiController]
    [Route("api/products/{productId:int}/images")]
    public class ProductImagesController : ControllerBase
    {
        private readonly ProductImageService _service;

        public ProductImagesController(ProductImageService service)
        {
            _service = service;
        }

        // -------------------- GET ALL (BY PRODUCT) --------------------
        [HttpGet]
        public async Task<IActionResult> GetAll(int productId, int page = 1, int pageSize = 10)
        {
            var response = await _service.GetAllByProductIdAsync(productId, page, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- GET BY ID --------------------
        [HttpGet("{imageId:int}")]
        public async Task<IActionResult> GetById(int productId, int imageId)
        {
            var response = await _service.GetByIdAsync(imageId);
            return StatusCode(response.StatusCode, response);
        }

    }
}
