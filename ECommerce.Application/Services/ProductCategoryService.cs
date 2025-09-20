using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Errors;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
namespace ECommerce.Application.Services
{
    public class ProductCategoryService
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IValidator<ProductCategoryDto> _productCategoryValidator;
        private readonly AutoMapper.IMapper _mapper;
        private readonly ILogger<ProductCategoryService> _logger;

        public ProductCategoryService(
            IProductCategoryRepository productCategoryRepository,
            ICategoryRepository categoryRepository,
            IProductRepository productRepository,
            IValidator<ProductCategoryDto> productCategoryValidator,
            AutoMapper.IMapper mapper,
            ILogger<ProductCategoryService> logger)
        {
            _productCategoryRepository = productCategoryRepository;
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _productCategoryValidator = productCategoryValidator;
            _mapper = mapper;
            _logger = logger;
        }

        // -------------------- Get By Product Id --------------------
        public async Task<AppResponse<List<ProductCategoryReadDto>>> GetProductCategoriesAsync(int productId)
        {
            try
            {
                if (!await DoesProductIDExist(productId))
                {
                    return AppResponse<List<ProductCategoryReadDto>>.ErrorResult(
                        new List<string> { $"Product with Id {productId} not found." },
                        (int)HttpStatusCode.NotFound
                    );
                }

                var productCategories = await _productCategoryRepository.GetByProductIdAsync(productId, asNoTracking: true);
                if (!productCategories.Any())
                {
                    return AppResponse<List<ProductCategoryReadDto>>.ErrorResult(
                        new List<string> { "No categories are assigned to this product." },
                        (int)HttpStatusCode.NotFound
                    );
                }

                var productCategoryDtos = productCategories
                    .Select(pc => _mapper.Map<ProductCategoryReadDto>(pc))
                    .ToList();

                return AppResponse<List<ProductCategoryReadDto>>.SuccessResult(productCategoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories for product {ProductId}", productId);
                return AppResponse<List<ProductCategoryReadDto>>.ErrorResult(
                    new List<string> { $"An unexpected error occurred while retrieving categories for product {productId}." },
                    (int)HttpStatusCode.InternalServerError
                );
            }
        }

        // -------------------- Get Single --------------------
        public async Task<AppResponse<ProductCategoryReadDto>> GetByProductAndCategoryAsync(int productId, int categoryId)
        {
            try
            {
                var entity = await _productCategoryRepository.GetByProductIdAndCategoryId(productId, categoryId);
                if (entity == null)
                {
                    return AppResponse<ProductCategoryReadDto>.ErrorResult(
                        new List<string> { $"No product-category assignment found for ProductId {productId} and CategoryId {categoryId}." },
                        (int)HttpStatusCode.NotFound
                    );
                }

                var dto = _mapper.Map<ProductCategoryReadDto>(entity);
                return AppResponse<ProductCategoryReadDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product-category assignment {ProductId}-{CategoryId}", productId, categoryId);
                return AppResponse<ProductCategoryReadDto>.ErrorResult(
                    new List<string> { "An unexpected error occurred while retrieving the product-category assignment." },
                    (int)HttpStatusCode.InternalServerError
                );
            }
        }

        // -------------------- CREATE --------------------
        public async Task<AppResponse<ProductCategoryReadDto>> CreateAsync(int productId, int categoryId)
        {
            try
            {
                var errors = new List<string>();

                if (!await DoesProductIDExist(productId))
                    errors.Add($"Product with Id {productId} not found.");

                if (!await DoesCategoryIDExist(categoryId))
                    errors.Add($"Category with Id {categoryId} not found.");
                if(await DoesProductCategoryExist(productId, categoryId))
                {
                    errors.Add($"Product With Id {productId} is already assigned to category with id {categoryId}");
                }

                if (errors.Any())
                {
                    return AppResponse<ProductCategoryReadDto>.ErrorResult(errors, (int)HttpStatusCode.NotFound);
                }

                var productCategory = new Domain.Entities.ProductCategory
                {
                    ProductId = productId,
                    CategoryId = categoryId,
                    CreatedAt = DateTime.UtcNow
                };

                var dto = _mapper.Map<ProductCategoryDto>(productCategory);
                var validationResult = await _productCategoryValidator.ValidateAsync(dto);

                if (!validationResult.IsValid)
                {
                    errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
                    return AppResponse<ProductCategoryReadDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);
                }

                await _productCategoryRepository.AddAsync(productCategory);

                var readDto = _mapper.Map<ProductCategoryReadDto>(productCategory);
                return AppResponse<ProductCategoryReadDto>.SuccessResult(readDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product-category assignment {ProductId}-{CategoryId}", productId, categoryId);
                return AppResponse<ProductCategoryReadDto>.ErrorResult(
                    new List<string> { "An unexpected error occurred while creating the product-category assignment." },
                    (int)HttpStatusCode.InternalServerError
                );
            }
        }

        // -------------------- ADD MULTIPLE --------------------
        public async Task<AppResponse<List<ProductCategoryReadDto>>> AddMultipleAsync(int productId, List<int> categoryIds)
        {
            try
            {
                var errors = new List<string>();

                if (!await DoesProductIDExist(productId))
                    errors.Add($"Product with Id {productId} not found.");

                foreach (var catId in categoryIds)
                {
                    if (!await DoesCategoryIDExist(catId))
                    {

                        errors.Add($"Category with Id {catId} not found.");
                    }
                    if (await DoesProductCategoryExist(productId, catId))
                    {
                        errors.Add($"Product With Id {productId} is already assigned to category with id {catId}");
                    }
                }

                if (errors.Any())
                    return AppResponse<List<ProductCategoryReadDto>>.ErrorResult(errors, (int)HttpStatusCode.NotFound);

                var readDtoList = new List<ProductCategoryReadDto>();
                foreach (var catId in categoryIds)
                {
                    readDtoList.Add(new ProductCategoryReadDto
                    {
                        ProductId = productId,
                        CategoryId = catId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _productCategoryRepository.AddCategoriesAsync(productId, categoryIds);
                return AppResponse<List<ProductCategoryReadDto>>.SuccessResult(readDtoList, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding multiple categories for product {ProductId}", productId);
                return AppResponse<List<ProductCategoryReadDto>>.ErrorResult(
                    new List<string> { "An unexpected error occurred while adding multiple categories." },
                    (int)HttpStatusCode.InternalServerError
                );
            }
        }

        // -------------------- REPLACE --------------------
        public async Task<AppResponse<List<ProductCategoryReadDto>>> ReplaceAsync(int productId, List<int> categoryIds)
        {
            try
            {
                var errors = new List<string>();

                if (!await DoesProductIDExist(productId))
                    errors.Add($"Product with Id {productId} not found.");

                foreach (var catId in categoryIds)
                {
                    if (!await DoesCategoryIDExist(catId))
                        errors.Add($"Category with Id {catId} not found.");
                }
                foreach (var catId in categoryIds)
                {
                    if (!await DoesProductCategoryExist(productId, catId))
                    {
                        errors.Add($"Product-category assignment for ProductId {productId} and CategoryId {catId} does not exist.");
                    }
                }
                if (errors.Any())
                    return AppResponse<List<ProductCategoryReadDto>>.ErrorResult(errors, (int)HttpStatusCode.NotFound);
                var readDtoList = new List<ProductCategoryReadDto>();
                foreach (var catId in categoryIds)
                {
                    readDtoList.Add(new ProductCategoryReadDto
                    {
                        ProductId = productId,
                        CategoryId = catId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _productCategoryRepository.ReplaceCategoriesAsync(productId, categoryIds);
                return AppResponse<List<ProductCategoryReadDto>>.SuccessResult(readDtoList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing categories for product {ProductId}", productId);
                return AppResponse<List<ProductCategoryReadDto>>.ErrorResult(
                    new List<string> { "An unexpected error occurred while replacing categories." },
                    (int)HttpStatusCode.InternalServerError
                );
            }
        }

        // -------------------- DELETE SINGLE --------------------
        public async Task<AppResponse<object>> DeleteAsync(int productId, int categoryId)
        {
            try
            {
                if (!await DoesProductCategoryExist(productId, categoryId))
                {
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"No product-category assignment found for ProductId {productId} and CategoryId {categoryId}." },
                        (int)HttpStatusCode.NotFound
                    );
                }

                await _productCategoryRepository.DeleteAsync(productId, categoryId);
                return AppResponse<object>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product-category assignment {ProductId}-{CategoryId}", productId, categoryId);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An unexpected error occurred while deleting the product-category assignment." },
                    (int)HttpStatusCode.InternalServerError
                );
            }
        }

        // -------------------- DELETE ALL BY PRODUCT --------------------
        public async Task<AppResponse<object>> DeleteByProductAsync(int productId)
        {
            try
            {
                if (!await DoesProductIDExist(productId))
                {
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"Product with Id {productId} not found." },
                        (int)HttpStatusCode.NotFound
                    );
                }

                await _productCategoryRepository.DeleteByProductIdAsync(productId);
                return AppResponse<object>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all product-category assignments for ProductId {ProductId}", productId);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An unexpected error occurred while deleting product-category assignments." },
                    (int)HttpStatusCode.InternalServerError
                );
            }
        }

        // -------------------- HELPERS --------------------
        private async Task<bool> DoesProductIDExist(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId, asNoTracking: true);
            return product != null;
        }

        private async Task<bool> DoesCategoryIDExist(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId, asNoTracking: true);
            return category != null;
        }

        private async Task<bool> DoesProductCategoryExist(int productId, int categoryId)
        {
            var pc = await _productCategoryRepository.GetByProductIdAndCategoryId(productId, categoryId);
            return pc != null;
        }
    }
}
