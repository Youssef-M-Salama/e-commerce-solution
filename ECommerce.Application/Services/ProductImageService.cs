using AutoMapper;
using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Errors;
using ECommerce.API.Admin.Application.Extensions;
using ECommerce.Application.Extensions;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Helpers;
using ECommerce.Domain.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace ECommerce.API.Admin.Application.Services
{
    public class ProductImageService
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductRepository _productRepository;
        private readonly IFileStorage _fileStorage;
        private readonly IValidator<ProductImageDto> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductImageService> _logger;

        public ProductImageService(
            IFileStorage fileStorage,
            IProductImageRepository productImageRepository,
            IProductRepository productRepository,
            IValidator<ProductImageDto> validator,
            IMapper mapper,
            ILogger<ProductImageService> logger)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _productImageRepository = productImageRepository ?? throw new ArgumentNullException(nameof(productImageRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // -------------------- GET ALL (BY PRODUCT) --------------------
        public async Task<AppPaginatedResponse<ProductImageReadDto>> GetAllByProductIdAsync(
            int productId, int page = 1, int pageSize = 10)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId, asNoTracking: true);
                if (product == null)
                {
                    return new AppPaginatedResponse<ProductImageReadDto>(
                        Enumerable.Empty<ProductImageReadDto>(),
                        new Pagination { CurrentPage = page, PageSize = pageSize, TotalItems = 0, TotalPages = 0 },
                        (int)HttpStatusCode.NotFound,
                        errors: new List<string> { Messages.GetNotFoundMessage("Product") }
                    );
                }

                var allImages = await _productImageRepository.GetAllByProductIdAsync(productId, asNoTracking: true);
                var totalImages = allImages.Count();

                var pagination = (totalImages, page, pageSize).BuildPagination();

                if (totalImages == 0)
                    return PaginationExtensions.EmptyPageResult<ProductImageReadDto>(pageSize);

                if (page > pagination.TotalPages && pagination.TotalPages > 0)
                    return pagination.NotFoundPageResult<ProductImageReadDto>();

                var pagedImages = await _productImageRepository.GetAllByProductIdAsync(
                    productId, page, pageSize, asNoTracking: true);

                var dtos = pagedImages.Select(i => _mapper.Map<ProductImageReadDto>(i)).ToList();

                return new AppPaginatedResponse<ProductImageReadDto>(
                    dtos,
                    pagination,
                    (int)HttpStatusCode.OK
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product images for ProductId {ProductId}", productId);
                return new AppPaginatedResponse<ProductImageReadDto>(
                    Enumerable.Empty<ProductImageReadDto>(),
                    new Pagination { CurrentPage = page, PageSize = pageSize, TotalItems = 0, TotalPages = 0 },
                    (int)HttpStatusCode.InternalServerError,
                    errors: new List<string> { Messages.GetErrorOccurredMessage() }
                );
            }
        }

        // -------------------- GET BY ID --------------------
        public async Task<AppResponse<ProductImageReadDto>> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _productImageRepository.GetByIdAsync(id, asNoTracking: true);
                if (entity == null)
                    return AppResponse<ProductImageReadDto>.ErrorResult(
                        new List<string> { Messages.GetNotFoundMessage("Product image") },
                        (int)HttpStatusCode.NotFound);

                var dto = _mapper.Map<ProductImageReadDto>(entity);
                return AppResponse<ProductImageReadDto>.SuccessResult(dto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product image {ImageId}", id);
                return AppResponse<ProductImageReadDto>.ErrorResult(
                    new List<string> { Messages.GetErrorOccurredMessage() },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- CREATE --------------------
        public async Task<AppResponse<ProductImageReadDto>> CreateAsync(int productId, ProductImageDto dto)
        {
            try
            {
                var errors = new List<string>();

                var product = await _productRepository.GetByIdAsync(productId, asNoTracking: true);
                if (product == null)
                    errors.Add(Messages.GetNotFoundMessage("Product"));

                var validation = await _validator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                if (await _productImageRepository.DoesDisplayOrderExistAsync(productId, dto.DisplayOrder))
                    errors.Add($"Display order {dto.DisplayOrder} already exists for this product.");

                if (dto.IsMain && await _productImageRepository.DoesMainImageExistAsync(productId))
                    errors.Add("A main image already exists for this product.");

                if (errors.Any())
                    return AppResponse<ProductImageReadDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);

                var entity = _mapper.Map<ProductImage>(dto);
                entity.ProductId = productId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.ImageUrl = await dto.ImageFile.SaveImageAsync(_fileStorage, "product-images");

                await _productImageRepository.AddAsync(entity);
                await UpdateProductMainImageAsync(productId);

                var createdDto = _mapper.Map<ProductImageReadDto>(entity);
                return AppResponse<ProductImageReadDto>.SuccessResult(createdDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product image for ProductId {ProductId}", productId);
                return AppResponse<ProductImageReadDto>.ErrorResult(
                    new List<string> { Messages.GetErrorOccurredMessage() },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- UPDATE --------------------
        public async Task<AppResponse<ProductImageReadDto>> UpdateAsync(int productId, int imageId, ProductImageDto dto)
        {
            try
            {
                var errors = new List<string>();

                var product = await _productRepository.GetByIdAsync(productId, asNoTracking: true);
                if (product == null)
                    errors.Add(Messages.GetNotFoundMessage("Product"));

                var entity = await _productImageRepository.GetByIdAsync(imageId, asNoTracking: false);
                if (entity == null || entity.ProductId != productId)
                    errors.Add(Messages.GetNotFoundMessage("Product image"));

                var validation = await _validator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                if (await _productImageRepository.DoesDisplayOrderExistAsync(productId, dto.DisplayOrder, excludeId: imageId))
                    errors.Add($"Display order {dto.DisplayOrder} already exists for this product.");

                if (dto.IsMain && await _productImageRepository.DoesMainImageExistAsync(productId, excludeId: imageId))
                    errors.Add("A main image already exists for this product.");

                if (errors.Any())
                {
                    var statusCode = errors.Any(e => e.Contains("not found"))
                        ? (int)HttpStatusCode.NotFound
                        : (int)HttpStatusCode.BadRequest;
                    return AppResponse<ProductImageReadDto>.ErrorResult(errors, statusCode);
                }

                entity.DisplayOrder = dto.DisplayOrder;
                entity.IsMain = dto.IsMain;
                entity.UpdatedAt = DateTime.UtcNow;

                if (dto.ImageFile != null)
                {
                    var extension = Path.GetExtension(dto.ImageFile.FileName).ToLowerInvariant();
                    using var stream = dto.ImageFile.OpenReadStream();
                    entity.ImageUrl = await _fileStorage.UpdateFileAsync(stream, extension, "product-images", entity.ImageUrl);
                }

                await _productImageRepository.UpdateAsync(entity);
                await UpdateProductMainImageAsync(productId);

                var updatedDto = _mapper.Map<ProductImageReadDto>(entity);
                return AppResponse<ProductImageReadDto>.SuccessResult(updatedDto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product image {ImageId}", imageId);
                return AppResponse<ProductImageReadDto>.ErrorResult(
                    new List<string> { Messages.GetErrorOccurredMessage() },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- SET MAIN --------------------
        public async Task<AppResponse<object>> SetMainAsync(int productId, int imageId)
        {
            try
            {
                var image = await _productImageRepository.GetByIdAsync(imageId);
                if (image == null || image.ProductId != productId)
                {
                    return AppResponse<object>.ErrorResult(
                        new List<string> { Messages.GetNotFoundMessage("Product image") },
                        (int)HttpStatusCode.BadRequest);
                }

                await _productImageRepository.SetMainAsync(productId, imageId, saveChanges: true);
                await UpdateProductMainImageAsync(productId);
                return AppResponse<object>.SuccessResult(null, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while setting main image for product {ProductId}.", productId);
                return AppResponse<object>.ErrorResult(
                    new List<string> { Messages.GetErrorOccurredMessage() },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- DELETE --------------------
        public async Task<AppResponse<object>> DeleteAsync(int productId, int imageId)
        {
            try
            {
                var entity = await _productImageRepository.GetByIdAsync(imageId, asNoTracking: false);
                if (entity == null || entity.ProductId != productId)
                {
                    return AppResponse<object>.ErrorResult(
                        new List<string> { Messages.GetNotFoundMessage("Product image") },
                        (int)HttpStatusCode.BadRequest);
                }

                await _productImageRepository.DeleteAsync(entity);
                await UpdateProductMainImageAsync(productId);

                return AppResponse<object>.SuccessResult(null, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product image {ImageId}", imageId);
                return AppResponse<object>.ErrorResult(
                    new List<string> { Messages.GetErrorOccurredMessage() },
                    (int)HttpStatusCode.InternalServerError);
            }
        }
        // ------------ -------- HELPERS --------------------
        private async Task UpdateProductMainImageAsync(int productId)
        {
            var mainImage = await _productImageRepository.GetMainOrFirstAsync(productId, asNoTracking: true);
            var product = await _productRepository.GetByIdAsync(productId, asNoTracking: false);
            if (product != null)
            {
                product.MainImageUrl = mainImage?.ImageUrl;
                await _productRepository.UpdateAsync(product);
            }
        }

    }


}
