using AutoMapper;
using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Errors;
using ECommerce.API.Admin.Application.Extensions;
using ECommerce.Application.Extensions;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Domain.Helpers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ECommerce.API.Admin.Application.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileStorage _fileStorage;
        private readonly IValidator<CategoryDto> _categoryValidator;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            IFileStorage fileStorage,
            ICategoryRepository categoryRepository,
            IValidator<CategoryDto> categoryValidator,
            IMapper mapper,
            ILogger<CategoryService> logger)
        {
            _fileStorage= fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _categoryValidator = categoryValidator ?? throw new ArgumentNullException(nameof(categoryValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // -------------------- GET ALL --------------------
        public async Task<AppPaginatedResponse<CategoryReadDto>> GetAllAsync(int page = 1, string search = "", int pageSize = 10)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Max(1, pageSize);

                var allCategories = await _categoryRepository.GetAllAsync(search, asNoTracking: true);
                var totalCategories = allCategories.Count();

                var pagination = (totalCategories, page, pageSize).BuildPagination();

                if (totalCategories == 0)
                    return PaginationExtensions.EmptyPageResult<CategoryReadDto>(pageSize);

                if (page > pagination.TotalPages && pagination.TotalPages > 0)
                    return pagination.NotFoundPageResult<CategoryReadDto>();

                var pagedCategories = await _categoryRepository.GetAllAsync(page, search ?? string.Empty, pageSize, asNoTracking: true);
                var categoryDtos = pagedCategories.Select(c => _mapper.Map<CategoryReadDto>(c)).ToList();

                return new AppPaginatedResponse<CategoryReadDto>(
                    categoryDtos,
                    pagination,
                    (int)HttpStatusCode.OK
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving categories.");
                return new AppPaginatedResponse<CategoryReadDto>(
                    Enumerable.Empty<CategoryReadDto>(),
                    new Pagination { CurrentPage = page, PageSize = pageSize, TotalItems = 0, TotalPages = 0 },
                    (int)HttpStatusCode.InternalServerError,
                    errors: new List<string> { "An error occurred while retrieving categories." }
                );
            }
        }

        // -------------------- GET BY ID --------------------
        public async Task<AppResponse<CategoryReadDto>> GetByIdAsync(int id)
        {
            try
            {
                var categoryEntity = await _categoryRepository.GetByIdAsync(id, asNoTracking: true);
                if (categoryEntity == null)
                    return AppResponse<CategoryReadDto>.ErrorResult(
                        new List<string> { $"Category with ID {id} not found." },
                        (int)HttpStatusCode.NotFound
                    );

                var categoryDto = _mapper.Map<CategoryReadDto>(categoryEntity);
                return AppResponse<CategoryReadDto>.SuccessResult(categoryDto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving category with ID {CategoryId}.", id);
                return AppResponse<CategoryReadDto>.ErrorResult(
                    new List<string> { "An error occurred while retrieving the category." },
                    (int)HttpStatusCode.InternalServerError
                );
            }
        }

        // -------------------- GET WITH CHILDREN --------------------
        public async Task<AppResponse<IEnumerable<CategoryChildDto>>> GetWithChildrenAsync(int parentId)
        {
            try
            {
                var children = await _categoryRepository.GetChildrenAsync(parentId, asNoTracking: true);

                if (children == null || !children.Any())
                    return AppResponse<IEnumerable<CategoryChildDto>>.ErrorResult(
                        new List<string> { $"No child categories found for category with ID {parentId}." },
                        (int)HttpStatusCode.NotFound
                    );

                var dto = _mapper.Map<IEnumerable<CategoryChildDto>>(children);
                return AppResponse<IEnumerable<CategoryChildDto>>.SuccessResult(dto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving children for category ID {CategoryId}", parentId);
                return AppResponse<IEnumerable<CategoryChildDto>>.ErrorResult(
                    new List<string> { "An error occurred while retrieving child categories." },
                    (int)HttpStatusCode.InternalServerError
                );
            }
        }

        // -------------------- CREATE --------------------
        public async Task<AppResponse<CategoryReadDto>> CreateAsync(CategoryDto categoryDto)
        {
            try
            {
                var validationResult = await _categoryValidator.ValidateAsync(categoryDto);
                if (!validationResult.IsValid)
                    return AppResponse<CategoryReadDto>.ErrorResult(
                        validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );

                if (await DoesCategoryNameExistAsync(categoryDto.Name))
                    return AppResponse<CategoryReadDto>.ErrorResult(
                        new List<string> { "Category name already exists." }
                    );

                if (categoryDto.ParentCategoryId.HasValue)
                {
                    if (categoryDto.ParentCategoryId == 0)
                        categoryDto.ParentCategoryId = null;

                    if (categoryDto.ParentCategoryId.HasValue)
                    {
                        var parent = await _categoryRepository.GetByIdAsync(categoryDto.ParentCategoryId.Value, asNoTracking: true);
                        if (parent == null)
                        {
                            return AppResponse<CategoryReadDto>.ErrorResult(
                                new List<string> { $"Parent category with ID {categoryDto.ParentCategoryId.Value} does not exist." }
                            );
                        }
                    }
                }

                var categoryEntity = _mapper.Map<Category>(categoryDto);

                if (categoryDto.ImageFile != null)
                {
                    categoryEntity.ImageUrl = await categoryDto.ImageFile.SaveImageAsync(_fileStorage, "categories");
                }

                var currentTime = DateTime.UtcNow;
                categoryEntity.Name = categoryEntity.Name?.Trim();
                categoryEntity.CreatedAt = currentTime;
                categoryEntity.UpdatedAt = currentTime;

                await _categoryRepository.AddAsync(categoryEntity, saveChanges: true);

                var createdDto = _mapper.Map<CategoryReadDto>(categoryEntity);
                createdDto.ParentCategoryName = await GetCategoryNameById(categoryEntity.ParentCategoryId ?? 0);
                return AppResponse<CategoryReadDto>.SuccessResult(createdDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating category with name {CategoryName}.", categoryDto?.Name);
                return AppResponse<CategoryReadDto>.ErrorResult(
                    new List<string> { "An error occurred while creating the category." },
                    (int)HttpStatusCode.InternalServerError
                );
            }
        }

        // -------------------- UPDATE --------------------
        public async Task<AppResponse<CategoryReadDto>> UpdateAsync(int id, CategoryDto categoryDto)
        {
            try
            {
                var validationResult = await _categoryValidator.ValidateAsync(categoryDto);
                if (!validationResult.IsValid)
                    return AppResponse<CategoryReadDto>.ErrorResult(
                        validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );

                var categoryEntity = await _categoryRepository.GetByIdAsync(id, asNoTracking: false);
                if (categoryEntity == null)
                    return AppResponse<CategoryReadDto>.ErrorResult(
                        new List<string> { $"Category with ID {id} not found." },
                        (int)HttpStatusCode.NotFound
                    );

                var newName = categoryDto.Name?.Trim();
                if (!string.Equals(categoryEntity.Name?.Trim(), newName, StringComparison.OrdinalIgnoreCase))
                {
                    if (await DoesCategoryNameExistAsync(newName))
                        return AppResponse<CategoryReadDto>.ErrorResult(
                            new List<string> { "Category name already exists." }
                        );
                }

                if (categoryDto.ParentCategoryId.HasValue)
                {
                    if (categoryDto.ParentCategoryId == id)
                        return AppResponse<CategoryReadDto>.ErrorResult(
                            new List<string> { "A category cannot be its own parent." }
                        );

                    var parent = await _categoryRepository.GetByIdAsync(categoryDto.ParentCategoryId.Value, asNoTracking: true);
                    if (parent == null)
                    {
                        return AppResponse<CategoryReadDto>.ErrorResult(
                            new List<string> { $"Parent category with ID {categoryDto.ParentCategoryId.Value} does not exist." }
                        );
                    }
                }

                categoryEntity.Name = newName;
                categoryEntity.Description = categoryDto.Description;
                categoryEntity.IsActive = categoryDto.IsActive;
                categoryEntity.ParentCategoryId = categoryDto.ParentCategoryId;
                categoryEntity.UpdatedAt = DateTime.UtcNow;

                if (categoryDto.ImageFile != null)
                {
                    var extension = Path.GetExtension(categoryDto.ImageFile.FileName).ToLowerInvariant();
                    using var stream = categoryDto.ImageFile.OpenReadStream();

                    categoryEntity.ImageUrl = await _fileStorage.UpdateFileAsync(stream, extension, "categories", categoryEntity.ImageUrl);
                }

                await _categoryRepository.UpdateAsync(categoryEntity, saveChanges: true);

                var updatedDto = _mapper.Map<CategoryReadDto>(categoryEntity);
                updatedDto.ParentCategoryName = await GetCategoryNameById(categoryEntity.ParentCategoryId ?? 0);
                return AppResponse<CategoryReadDto>.SuccessResult(updatedDto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating category with ID {CategoryId}.", id);
                return AppResponse<CategoryReadDto>.ErrorResult(
                    new List<string> { "An error occurred while updating the category." },
                    (int)HttpStatusCode.InternalServerError
                );
            }
        }

        // -------------------- DELETE --------------------
        public async Task<AppResponse<object>> DeleteAsync(int id)
        {
            try
            {
                var errors = new List<string>();
                var categoryEntity = await _categoryRepository.GetWithChildrenAsync(id, asNoTracking: false);
                if (categoryEntity == null)
                {
                    errors.Add($"Category with ID {id} not found.");
                }
                if (categoryEntity.SubCategories.Any())
                {
                    errors.Add("Cannot delete category with existing sub-categories. Please remove or reassign them first.");
                }
                if (errors.Any())
                {
                    var statusCode = errors.Any(e => e.Contains("not found"))
                        ? (int)HttpStatusCode.NotFound
                        : (int)HttpStatusCode.BadRequest;
                    return AppResponse<object>.ErrorResult(errors, statusCode);
                }

                await _categoryRepository.DeleteAsync(categoryEntity, saveChanges: true);
                return AppResponse<object>.SuccessResult(null, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting category with ID {CategoryId}.", id);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while deleting the category." },
                    (int)HttpStatusCode.InternalServerError
                );
            }
        }

        // -------------------- HELPERS --------------------
        private async Task<bool> DoesCategoryNameExistAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return false;

            categoryName = categoryName.Trim();

            var candidateCategories = (await _categoryRepository
                .GetAllAsync(page: 1, search: categoryName, pageSize: 20, asNoTracking: true))
                .ToList();

            var existsInCandidates = candidateCategories.Any(c =>
                string.Equals(c.Name?.Trim(), categoryName, StringComparison.OrdinalIgnoreCase));

            if (existsInCandidates)
                return true;

            var allCategories = await _categoryRepository.GetAllAsync(asNoTracking: true);
            return allCategories.Any(c =>
                string.Equals(c.Name?.Trim(), categoryName, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<string>GetCategoryNameById(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id, asNoTracking: true);
            return category?.Name ?? string.Empty;
        }
    }
}
