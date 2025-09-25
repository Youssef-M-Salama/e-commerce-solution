using AutoMapper;
using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Errors;
using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces.Repositories;
using ECommerce.Domain.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ECommerce.API.Admin.Application.Services
{
    public class WishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<WishlistCreateDto> _wishlistValidator;
        private readonly IValidator<WishlistItemCreateDto> _wishlistItemValidator;
        private readonly IValidator<WishlistItemDeleteDto> _wishlistItemDeleteValidator;
        private readonly ILogger<WishlistService> _logger;

        public WishlistService(
            IUserRepository userRepository,
            IWishlistRepository wishlistRepository,
            IProductRepository productRepository,
            IMapper mapper,
            IValidator<WishlistCreateDto> wishlistValidator,
            IValidator<WishlistItemCreateDto> wishlistItemValidator,
            IValidator<WishlistItemDeleteDto> wishlistItemDeleteValidator,
            ILogger<WishlistService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _wishlistRepository = wishlistRepository ?? throw new ArgumentNullException(nameof(wishlistRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _wishlistValidator = wishlistValidator ?? throw new ArgumentNullException(nameof(wishlistValidator));
            _wishlistItemValidator = wishlistItemValidator ?? throw new ArgumentNullException(nameof(wishlistItemValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _wishlistItemDeleteValidator=wishlistItemDeleteValidator??throw new ArgumentException(nameof(wishlistItemDeleteValidator));
        }

        // -------------------- GET BY USER --------------------
        public async Task<AppResponse<WishlistReadDto>> GetByUserIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, asNoTracking: true);
                if (user == null)
                    return AppResponse<WishlistReadDto>.ErrorResult(
                        new List<string> { $"User {userId} not found." },
                        (int)HttpStatusCode.NotFound);

                var wishlistId = await _wishlistRepository.GetUserWishlistId(userId);
                if (wishlistId == null)
                    return AppResponse<WishlistReadDto>.ErrorResult(
                        new List<string> { $"Wishlist for user {userId} not found." },
                        (int)HttpStatusCode.NotFound);

                var items = await _wishlistRepository.GetByUserIdAsync(userId, asNoTracking: true);

                var wishlistDto = new WishlistReadDto
                {
                    WishListId = wishlistId.Value,
                    UserId = userId,
                    Items = items.Select(i => new WishlistItemReadDto
                    {
                        WishListItemId = i.Id,
                        Product = _mapper.Map<ProductReadDto>(i.Product),
                        CreatedAt = i.CreatedAt
                    }).ToList()
                };

                return AppResponse<WishlistReadDto>.SuccessResult(wishlistDto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wishlist for user {UserId}", userId);
                return AppResponse<WishlistReadDto>.ErrorResult(
                    new List<string> { "An error occurred while retrieving wishlist." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- CREATE WISHLIST --------------------
        public async Task<AppResponse<WishlistReadDto>> CreateWishlistAsync(WishlistCreateDto dto)
        {
            try
            {
                var validation = await _wishlistValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    return AppResponse<WishlistReadDto>.ErrorResult(
                        validation.Errors.Select(e => e.ErrorMessage).ToList(),
                        (int)HttpStatusCode.BadRequest);

                var user = await _userRepository.GetByIdAsync(dto.UserId, asNoTracking: true);
                if (user == null)
                    return AppResponse<WishlistReadDto>.ErrorResult(
                        new List<string> { $"User {dto.UserId} not found." },
                        (int)HttpStatusCode.NotFound);

                if (await _wishlistRepository.GetUserWishlistId(dto.UserId) != null)
                    return AppResponse<WishlistReadDto>.ErrorResult(
                        new List<string> { $"User {dto.UserId} already has a wishlist." },
                        (int)HttpStatusCode.BadRequest);

                var wishlist = new Wishlist
                {
                    UserId = dto.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _wishlistRepository.CreateWishlistAsync(wishlist, saveChanges: true);

                var readDto = _mapper.Map<WishlistReadDto>(wishlist);
                return AppResponse<WishlistReadDto>.SuccessResult(readDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating wishlist for user {UserId}", dto?.UserId);
                return AppResponse<WishlistReadDto>.ErrorResult(
                    new List<string> { "An error occurred while creating wishlist." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- DELETE WISHLIST --------------------
        public async Task<AppResponse<object>> DeleteWishlistAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, asNoTracking: true);
                if (user == null)
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"User {userId} not found." },
                        (int)HttpStatusCode.NotFound);

                var deleted = await _wishlistRepository.DeleteWishlistAsync(userId, saveChanges: true);
                if (!deleted)
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"Wishlist for user {userId} not found." },
                        (int)HttpStatusCode.NotFound);

                return AppResponse<object>.SuccessResult(null, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting wishlist for user {UserId}", userId);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while deleting wishlist." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- ADD ITEM --------------------
        public async Task<AppResponse<WishlistItemReadDto>> AddItemAsync(WishlistItemCreateDto dto)
        {
            try
            {
                var validation = await _wishlistItemValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                {
                    return AppResponse<WishlistItemReadDto>.ErrorResult(
                        validation.Errors.Select(e => e.ErrorMessage).ToList(),
                        (int)HttpStatusCode.BadRequest);
                }

                var product = await _productRepository.GetByIdAsync(dto.ProductId, asNoTracking: true);
                if (product == null)
                {
                    return AppResponse<WishlistItemReadDto>.ErrorResult(
                        new List<string> { $"Product {dto.ProductId} not found." },
                        (int)HttpStatusCode.NotFound);
                }
                if (!await _wishlistRepository.IsUniqueItem(dto.WishlistId, dto.ProductId))
                {
                    return AppResponse<WishlistItemReadDto>.ErrorResult(new List<string> { "prouct is already in the wishlist" }, (int)HttpStatusCode.BadRequest);
                }
                var item = new WishlistItem
                {
                    WishlistId = dto.WishlistId,
                    ProductId = dto.ProductId,
                    CreatedAt = DateTime.UtcNow
                };

                await _wishlistRepository.AddItemAsync(item, saveChanges: true);

                var readDto = new WishlistItemReadDto
                {
                    WishListItemId = item.Id,
                    Product = _mapper.Map<ProductReadDto>(product),
                    CreatedAt = item.CreatedAt
                };

                return AppResponse<WishlistItemReadDto>.SuccessResult(readDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to wishlist {WishlistId}", dto?.WishlistId);
                return AppResponse<WishlistItemReadDto>.ErrorResult(
                    new List<string> { "An error occurred while adding item to wishlist." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- REMOVE ITEM --------------------
        public async Task<AppResponse<object>> RemoveItemAsync(WishlistItemDeleteDto dto)
        {
            try
            {

                var validation = await _wishlistItemDeleteValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                {
                    var ValidatorErrors = validation.Errors.Select(e => e.ErrorMessage).ToList();
                    return AppResponse<object>.ErrorResult(ValidatorErrors, (int)HttpStatusCode.BadRequest);
                }
                var errors=new List<string>();

                var product = await _productRepository.GetByIdAsync(dto.ProductId, asNoTracking: true);
                if (product == null)
                {
                    errors.Add($"Product {dto.ProductId} not found.");
                }

                 if(!await _wishlistRepository.IsWishlistExist(dto.WishlistId))
                 {
                    errors.Add($"Wishlist {dto.WishlistId} not found.");
                 }
                if (errors.Any())
                {
                    return AppResponse<object>.ErrorResult(
                       errors,
                       (int)HttpStatusCode.NotFound);
                }
                // -------------------- DELETE ITEM --------------------
                var deleted = await _wishlistRepository.DeleteItemAsync(dto.WishlistId,dto.ProductId, saveChanges: true);
                if (!deleted)
                {
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"Item not found in wishlist {dto.WishlistId}." },
                        (int)HttpStatusCode.NotFound);
                }

                return AppResponse<object>.SuccessResult(null, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item {ProductId} from wishlist {WishlistId}", dto.ProductId, dto.WishlistId);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while removing item from wishlist." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }


    }
}
 