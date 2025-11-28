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
    public class BrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IFileStorage _fileStorage;
        private readonly IValidator<BrandDto> _brandValidator;
        private readonly IMapper _mapper;
        private readonly ILogger<BrandService> _logger;

        public BrandService(
            IFileStorage fileStorage,
            IBrandRepository brandRepository,
            IValidator<BrandDto> brandValidator,
            IMapper mapper,
            ILogger<BrandService> logger)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
            _brandValidator = brandValidator ?? throw new ArgumentNullException(nameof(brandValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // -------------------- GET ALL --------------------
        public async Task<AppPaginatedResponse<BrandReadDto>> GetAllAsync(int page = 1, string search = "", int pageSize = 10)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Max(1, pageSize);

                var allBrands = await _brandRepository.GetAllAsync(search, asNoTracking: true);
                var totalBrands = allBrands.Count();

                var pagination = (totalBrands, page, pageSize).BuildPagination();

                if (page > pagination.TotalPages && pagination.TotalPages > 0)
                    return pagination.NotFoundPageResult<BrandReadDto>();

                if (totalBrands == 0)
                    return PaginationExtensions.EmptyPageResult<BrandReadDto>(pageSize);


                var pagedBrands = await _brandRepository.GetAllAsync(page, search ?? string.Empty, pageSize, asNoTracking: true);
                var brandDtos = pagedBrands.Select(b => _mapper.Map<BrandReadDto>(b)).ToList();

                return new AppPaginatedResponse<BrandReadDto>(
                    brandDtos,
                    pagination,
                    (int)HttpStatusCode.OK
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving brands.");
                return new AppPaginatedResponse<BrandReadDto>(
                    Enumerable.Empty<BrandReadDto>(),
                    new Pagination { CurrentPage = page, PageSize = pageSize, TotalItems = 0, TotalPages = 0 },
                    (int)HttpStatusCode.InternalServerError,
                    errors: new List<string> { "An error occurred while retrieving brands." }
                );
            }
        }

        // -------------------- GET BY ID --------------------
        public async Task<AppResponse<BrandReadDto>> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _brandRepository.GetByIdAsync(id, asNoTracking: true);
                if (entity == null)
                    return AppResponse<BrandReadDto>.ErrorResult(
                        new List<string> { $"Brand with Id {id} not found." },
                        (int)HttpStatusCode.NotFound);

                var dto = _mapper.Map<BrandReadDto>(entity);
                return AppResponse<BrandReadDto>.SuccessResult(dto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving brand with ID {BrandId}", id);
                return AppResponse<BrandReadDto>.ErrorResult(
                    new List<string> { "An error occurred while retrieving the brand." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- CREATE --------------------
        public async Task<AppResponse<BrandReadDto>> CreateAsync(BrandDto brandDto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _brandValidator.ValidateAsync(brandDto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                if (await DoesBrandNameExistAsync(brandDto.Name))
                    errors.Add("Brand name already exists.");

                if (errors.Any())
                    return AppResponse<BrandReadDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);

                var entity = _mapper.Map<Brand>(brandDto);

                if (brandDto.ImageFile != null)
                    entity.LogoUrl = await brandDto.ImageFile.SaveImageAsync(_fileStorage, "brands");

                entity.Name = entity.Name?.Trim();
                entity.Description = entity.Description?.Trim();
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                await _brandRepository.AddAsync(entity, saveChanges: true);

                var createdDto = _mapper.Map<BrandReadDto>(entity);
                return AppResponse<BrandReadDto>.SuccessResult(createdDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand {BrandName}", brandDto?.Name);
                return AppResponse<BrandReadDto>.ErrorResult(
                    new List<string> { "An error occurred while creating the brand." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- UPDATE --------------------
        public async Task<AppResponse<BrandReadDto>> UpdateAsync(int id, BrandDto brandDto)
        {
            try
            {
                var errors = new List<string>();

                var entity = await _brandRepository.GetByIdAsync(id, asNoTracking: false);
                if (entity == null)
                    errors.Add($"Brand with Id {id} not found.");

                var validation = await _brandValidator.ValidateAsync(brandDto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                var newName = brandDto.Name?.Trim();
                if (entity != null && !string.Equals(entity?.Name?.Trim(), newName, StringComparison.OrdinalIgnoreCase))
                {
                    if (await DoesBrandNameExistAsync(newName))
                        errors.Add("Brand name already exists.");
                }

                if (errors.Any())
                {
                    var statusCode = errors.Any(e => e.Contains("not found"))
                        ? (int)HttpStatusCode.NotFound
                        : (int)HttpStatusCode.BadRequest;
                    return AppResponse<BrandReadDto>.ErrorResult(errors, statusCode);
                }

                entity!.Name = newName;
                entity.Description = brandDto.Description;
                entity.IsActive = brandDto.IsActive;
                entity.UpdatedAt = DateTime.UtcNow;

                if (brandDto.ImageFile != null)
                {
                    var extension = Path.GetExtension(brandDto.ImageFile.FileName).ToLowerInvariant();
                    using var stream = brandDto.ImageFile.OpenReadStream();
                    entity.LogoUrl = await _fileStorage.UpdateFileAsync(stream, extension, "brands", entity.LogoUrl);
                }

                await _brandRepository.UpdateAsync(entity, saveChanges: true);

                var updatedDto = _mapper.Map<BrandReadDto>(entity);
                return AppResponse<BrandReadDto>.SuccessResult(updatedDto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand {BrandId}", id);
                return AppResponse<BrandReadDto>.ErrorResult(
                    new List<string> { "An error occurred while updating the brand." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- DELETE --------------------
        public async Task<AppResponse<object>> DeleteAsync(int id)
        {
            try
            {
                var entity = await _brandRepository.GetByIdAsync(id, asNoTracking: false);
                if (entity == null)
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"Brand with Id {id} not found." },
                        (int)HttpStatusCode.NotFound);

                await _brandRepository.DeleteAsync(entity, saveChanges: true);
                return AppResponse<object>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand {BrandId}", id);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while deleting the brand." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }


        // -------------------- HELPERS --------------------
        private async Task<bool> DoesBrandNameExistAsync(string brandName)
        {
            if (string.IsNullOrWhiteSpace(brandName))
                return false;

            brandName = brandName.Trim();

            var candidateBrands = (await _brandRepository
                .GetAllAsync(page: 1, search: brandName, pageSize: 20, asNoTracking: true))
                .ToList();

            var existsInCandidates = candidateBrands.Any(b =>
                string.Equals(b.Name?.Trim(), brandName, StringComparison.OrdinalIgnoreCase));

            if (existsInCandidates)
                return true;

            var allBrands = await _brandRepository.GetAllAsync(asNoTracking: true);
            return allBrands.Any(b =>
                string.Equals(b.Name?.Trim(), brandName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
