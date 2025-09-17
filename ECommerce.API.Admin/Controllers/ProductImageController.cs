using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/products/{productId:int}/images")]
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

        // -------------------- CREATE --------------------
        [HttpPost]
        public async Task<IActionResult> Create(int productId, [FromForm] ProductImageDto dto)
        {
            var response = await _service.CreateAsync(productId, dto);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- UPDATE --------------------
        [HttpPut("{imageId:int}")]
        public async Task<IActionResult> Update(int productId, int imageId, [FromForm] ProductImageDto dto)
        {
            var response = await _service.UpdateAsync(productId, imageId, dto);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- SET MAIN --------------------
        [HttpPatch("{imageId:int}/set-main")]
        public async Task<IActionResult> SetMain(int productId, int imageId)
        {
            var response = await _service.SetMainAsync(productId, imageId);
            return StatusCode(response.StatusCode, response);
        }

        // -------------------- DELETE --------------------
        [HttpDelete("{imageId:int}")]
        public async Task<IActionResult> Delete(int productId, int imageId)
        {
            var response = await _service.DeleteAsync(productId, imageId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
