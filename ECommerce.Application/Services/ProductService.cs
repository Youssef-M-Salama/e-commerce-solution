using AutoMapper;
using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Errors;
using ECommerce.API.Admin.Application.Extensions;
using ECommerce.Application.Extensions;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ECommerce.Application.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IValidator<ProductDto> _productValidator;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            IValidator<ProductDto> productValidator,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
            _productValidator = productValidator ?? throw new ArgumentNullException(nameof(productValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // -------------------- GET ALL --------------------
        public async Task<AppPaginatedResponse<ProductReadDto>> GetAllAsync(
            int page = 1, string search = "", int? categoryId = null, int? brandId = null, int pageSize = 10)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Max(1, pageSize);

                var allProducts = await _productRepository.GetAllAsync(search, asNoTracking: true);
                var totalProducts = allProducts.Count();

                var pagination = (totalProducts, page, pageSize).BuildPagination();

                if (totalProducts == 0)
                    return PaginationExtensions.EmptyPageResult<ProductReadDto>(pageSize);

                if (page > pagination.TotalPages && pagination.TotalPages > 0)
                    return pagination.NotFoundPageResult<ProductReadDto>();

                var pagedProducts = await _productRepository.GetAllAsync(
                    page, search ?? string.Empty, categoryId, brandId, pageSize, asNoTracking: true);

                var dtos = pagedProducts.Select(p => _mapper.Map<ProductReadDto>(p)).ToList();

                return new AppPaginatedResponse<ProductReadDto>(
                    dtos,
                    pagination,
                    (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving products.");
                return new AppPaginatedResponse<ProductReadDto>(
                    Enumerable.Empty<ProductReadDto>(),
                    new Pagination { CurrentPage = page, PageSize = pageSize, TotalItems = 0, TotalPages = 0 },
                    (int)HttpStatusCode.InternalServerError,
                    errors: new List<string> { Messages.GetErrorOccurredMessage() });
            }
        }

        // -------------------- GET BY ID --------------------
        public async Task<AppResponse<ProductReadDto>> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _productRepository.GetByIdAsync(id, asNoTracking: true);
                if (entity == null)
                {
                    return AppResponse<ProductReadDto>.ErrorResult(
                        new List<string> { Messages.GetNotFoundMessage("Product") },
                        (int)HttpStatusCode.NotFound);
                }

                var dto = _mapper.Map<ProductReadDto>(entity);
                return AppResponse<ProductReadDto>.SuccessResult(dto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving product {ProductId}", id);
                return AppResponse<ProductReadDto>.ErrorResult(
                    new List<string> { Messages.GetErrorOccurredMessage() },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- CREATE --------------------
        public async Task<AppResponse<ProductReadDto>> CreateAsync(ProductDto dto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _productValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                if (dto.BrandId.HasValue && !await DoesBrandExistAsync(dto.BrandId.Value))
                    errors.Add(Messages.GetForeignKeyNotFoundMessage("Brand", dto.BrandId.Value));

                if (errors.Any())
                    return AppResponse<ProductReadDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);

                var entity = _mapper.Map<Product>(dto);
                var now = DateTime.UtcNow;
                entity.CreatedAt = now;
                entity.UpdatedAt = now;
                entity.Name = entity.Name?.Trim();
                entity.Description = entity.Description?.Trim();

                await _productRepository.AddAsync(entity);

                var createdDto = _mapper.Map<ProductReadDto>(entity);
                if (dto.BrandId.HasValue)
                    createdDto.BrandName = await GetProductBrandNameAsync(dto.BrandId.Value);

                return AppResponse<ProductReadDto>.SuccessResult(createdDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating product.");
                return AppResponse<ProductReadDto>.ErrorResult(
                    new List<string> { Messages.GetErrorOccurredMessage() },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- UPDATE --------------------
        public async Task<AppResponse<ProductReadDto>> UpdateAsync(int id, ProductDto dto)
        {
            try
            {
                var errors = new List<string>();

                var entity = await _productRepository.GetByIdAsync(id, asNoTracking: false);
                if (entity == null)
                    errors.Add(Messages.GetNotFoundMessage("Product"));

                var validation = await _productValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                if (dto.BrandId.HasValue && !await DoesBrandExistAsync(dto.BrandId.Value))
                    errors.Add(Messages.GetForeignKeyNotFoundMessage("Brand", dto.BrandId.Value));

                if (errors.Any())
                {
                    var statusCode = errors.Any(e => e.Contains("not found"))
                        ? (int)HttpStatusCode.NotFound
                        : (int)HttpStatusCode.BadRequest;
                    return AppResponse<ProductReadDto>.ErrorResult(errors, statusCode);
                }

                var now = DateTime.UtcNow;
                entity.Name = dto.Name?.Trim();
                entity.Description = dto.Description?.Trim();
                entity.Price = dto.Price;
                entity.BrandId = dto.BrandId;
                entity.UpdatedAt = now;

                await _productRepository.UpdateAsync(entity);

                var updatedDto = _mapper.Map<ProductReadDto>(entity);
                if (dto.BrandId.HasValue)
                    updatedDto.BrandName = await GetProductBrandNameAsync(dto.BrandId.Value);

                return AppResponse<ProductReadDto>.SuccessResult(updatedDto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product {ProductId}", id);
                return AppResponse<ProductReadDto>.ErrorResult(
                    new List<string> { Messages.GetErrorOccurredMessage() },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- DELETE --------------------
        public async Task<AppResponse<object>> DeleteAsync(int id)
        {
            try
            {
                var entity = await _productRepository.GetByIdAsync(id, asNoTracking: false);
                if (entity == null)
                {
                    return AppResponse<object>.ErrorResult(
                        new List<string> { Messages.GetNotFoundMessage("Product") },
                        (int)HttpStatusCode.NotFound);
                }

                await _productRepository.DeleteAsync(entity);
                return AppResponse<object>.SuccessResult(null, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product {ProductId}", id);
                return AppResponse<object>.ErrorResult(
                    new List<string> { Messages.GetErrorOccurredMessage() },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- HELPERS --------------------
        private async Task<string> GetProductBrandNameAsync(int brandId)
        {
            var brand = await _brandRepository.GetByIdAsync(brandId, asNoTracking: true);
            return brand?.Name ?? "Unknown Brand";
        }

        private async Task<bool> DoesBrandExistAsync(int brandId)
        {
            var brand = await _brandRepository.GetByIdAsync(brandId, asNoTracking: true);
            return brand != null;
        }
    }
}
