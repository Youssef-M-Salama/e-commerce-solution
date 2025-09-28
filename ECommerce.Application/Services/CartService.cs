using AutoMapper;
using ECommerce.API.Admin.Application.Errors;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ECommerce.API.Admin.Application.Services
{
    public class CartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CartItemCreateDto> _createValidator;
        private readonly IValidator<CartItemUpdateDto> _updateValidator;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IValidator<CartItemCreateDto> createValidator,
            IValidator<CartItemUpdateDto> updateValidator,
            ILogger<CartService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        // -------------------- CREATE CART --------------------
        public async Task<AppResponse<object>> CreateAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return AppResponse<object>.ErrorResult(new List<string> { $"User with id {userId} not found" }, (int)HttpStatusCode.NotFound);
                }

                var userCart = await _cartRepository.GetUserCartId(userId);
                if (userCart != null)
                {
                    return AppResponse<object>.ErrorResult(new List<string> { $"User with id {userId} already has a cart" }, (int)HttpStatusCode.BadRequest);
                }

                var cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                await _cartRepository.CreateAsync(cart);
                var dto = _mapper.Map<CartReadDto>(cart);
                return AppResponse<object>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cart for user {UserId}", userId);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while creating cart." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }     
        
        // -------------------- DELETE CART --------------------
        public async Task<AppResponse<object>> DeleteAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return AppResponse<object>.ErrorResult(new List<string> { $"User with id {userId} not found" }, (int)HttpStatusCode.NotFound);
                }

                var userCart = await _cartRepository.GetUserCartId(userId);
                if (userCart == null)
                {
                    return AppResponse<object>.ErrorResult(new List<string> { $"User with id {userId} doesn't have a cart" }, (int)HttpStatusCode.BadRequest);
                }

                var cart = new Cart
                {
                    Id = userCart.Value,
                    UserId = userId,
                };
                await _cartRepository.DeleteAsync(cart);
                return AppResponse<object>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cart for user {UserId}", userId);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while deleting cart." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }
        // -------------------- GET CART ITEMS  --------------------
        public async Task<AppResponse<List<CartItemReadDto>>> GetUserCartAsync(int userId)
        {
            try
            {
                var userCartId=await _cartRepository.GetUserCartId(userId);
                if (userCartId == null)
                {
                    return AppResponse<List<CartItemReadDto>>.ErrorResult(new List<string> { $"User with id {userId} not found"},(int)HttpStatusCode.NotFound);
                }
                var items = await _cartRepository.GetByUserIdAsync(userId, asNoTracking: true);
                var dto = _mapper.Map<List<CartItemReadDto>>(items);
                return AppResponse<List<CartItemReadDto>>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}", userId);
                return AppResponse<List<CartItemReadDto>>.ErrorResult(
                    new List<string> { "An error occurred while retrieving cart." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- ADD ITEM --------------------
        public async Task<AppResponse<CartItemReadDto>> AddItemAsync(int userId, CartItemCreateDto dto)
        {
            try
            {
                var validation = await _createValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    return AppResponse<CartItemReadDto>.ErrorResult(
                        validation.Errors.Select(e => e.ErrorMessage).ToList(),
                        (int)HttpStatusCode.BadRequest);
                var user= await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return AppResponse<CartItemReadDto>.ErrorResult(
                        new List<string> { $"User {userId} not found." },
                        (int)HttpStatusCode.NotFound);
                }
                var product = await _productRepository.GetByIdAsync(dto.ProductId);
                if (product == null)
                    return AppResponse<CartItemReadDto>.ErrorResult(
                        new List<string> { $"Product {dto.ProductId} not found." },
                        (int)HttpStatusCode.NotFound);

                if (dto.Quantity > product.Stock)
                    return AppResponse<CartItemReadDto>.ErrorResult(
                        new List<string> { "Not enough stock." },
                        (int)HttpStatusCode.BadRequest);

                var cartId = await _cartRepository.GetUserCartId(userId) ?? 0;
                if (cartId == 0)
                {
                    var cart = new Cart
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _cartRepository.CreateAsync(cart);
                    cartId = cart.Id;
                }

                if (!await _cartRepository.IsUniqueItem(cartId, dto.ProductId))
                    return AppResponse<CartItemReadDto>.ErrorResult(
                        new List<string> { "Item already in cart." },
                        400);

                var cartItem = _mapper.Map<CartItem>(dto);
                cartItem.CartId = cartId;
                cartItem.UnitPrice = product.Price;
                await _cartRepository.AddItemAsync(cartItem);

                var readDto = _mapper.Map<CartItemReadDto>(cartItem);
                return AppResponse<CartItemReadDto>.SuccessResult(readDto, 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item {ProductId} to cart for user {UserId}", dto.ProductId, userId);
                return AppResponse<CartItemReadDto>.ErrorResult(
                    new List<string> { "An error occurred while adding item to cart." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- REMOVE ITEM --------------------
        public async Task<AppResponse<object>> RemoveItemAsync(int userId, int productId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"User {userId} not found." },
                        (int)HttpStatusCode.NotFound);
                }
                var cartId = await _cartRepository.GetUserCartId(userId) ?? 0;
                if (cartId == 0)
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"Cart not found for user {userId}." },
                        404);

                var deleted = await _cartRepository.DeleteAsync(cartId, productId);
                if (!deleted)
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"Item not found in cart." },
                        404);

                return AppResponse<object>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product {ProductId} from cart for user {UserId}", productId, userId);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while removing item from cart." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- UPDATE QUANTITY --------------------
        public async Task<AppResponse<bool>> UpdateQuantityAsync(int userId, CartItemUpdateDto dto)
        {
            try
            {
                var validation = await _updateValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    return AppResponse<bool>.ErrorResult(
                        validation.Errors.Select(e => e.ErrorMessage).ToList(),
                        400);
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return AppResponse<bool>.ErrorResult(
                        new List<string> { $"User {userId} not found." },
                        (int)HttpStatusCode.NotFound);
                }

                var product = await _productRepository.GetByIdAsync(dto.ProductId);
                if (product == null)
                    return AppResponse<bool>.ErrorResult(
                        new List<string> { $"Product {dto.ProductId} not found." },
                        (int)HttpStatusCode.NotFound);

                if (dto.Quantity > product.Stock)
                    return AppResponse<bool>.ErrorResult(
                        new List<string> { "Quantity exceeds available stock." },
                        (int)HttpStatusCode.BadRequest);

                var cartId = await _cartRepository.GetUserCartId(userId) ?? 0;
                if (cartId == 0)
                    return AppResponse<bool>.ErrorResult(
                        new List<string> { $"Cart not found for user {userId}." },
                        (int)HttpStatusCode.NotFound);

                await _cartRepository.updateItemQuantityAsync(cartId, dto.ProductId, dto.Quantity);
                return AppResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity for product {ProductId} in cart for user {UserId}", dto.ProductId, userId);
                return AppResponse<bool>.ErrorResult(
                    new List<string> { "An error occurred while updating cart item quantity." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
