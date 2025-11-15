using AutoMapper;
using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Errors;
using ECommerce.API.Admin.Application.Extensions;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ECommerce.Customer.API.Application.Services
{
    public class CustomerOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IValidator<CreateCashOrderDto> _createCashOrderValidator;
        private readonly IValidator<CreateCheckoutOrderDto> _createCheckoutOrderValidator;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerOrderService> _logger;

        public CustomerOrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IValidator<CreateCashOrderDto> createCashOrderValidator,
            IValidator<CreateCheckoutOrderDto> createCheckoutOrderValidator,
            IMapper mapper,
            ILogger<CustomerOrderService> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _createCashOrderValidator = createCashOrderValidator ?? throw new ArgumentNullException(nameof(createCashOrderValidator));
            _createCheckoutOrderValidator = createCheckoutOrderValidator ?? throw new ArgumentNullException(nameof(createCheckoutOrderValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AppPaginatedResponse<OrderReadDto>> GetUserOrdersAsync(int userId, int page = 1, int pageSize = 10)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Max(1, pageSize);

                var allOrders = await _orderRepository.GetUserOrdersAsync(userId, asNoTracking: true);
                var totalOrders = allOrders.Count();

                var pagination = (totalOrders, page, pageSize).BuildPagination();

                if (totalOrders == 0)
                    return PaginationExtensions.EmptyPageResult<OrderReadDto>(pageSize);

                if (page > pagination.TotalPages && pagination.TotalPages > 0)
                    return pagination.NotFoundPageResult<OrderReadDto>();

                var pagedOrders = await _orderRepository.GetUserOrdersAsync(userId, page, pageSize, asNoTracking: true);
                var orderDtos = new List<OrderReadDto>();

                foreach (var order in pagedOrders)
                {
                    var orderDto = _mapper.Map<OrderReadDto>(order);
                    var orderWithItems = await _orderRepository.GetOrderWithItemsAsync(order.Id, asNoTracking: true);
                    if (orderWithItems != null)
                        orderDto.OrderItems = _mapper.Map<List<OrderItemReadDto>>(orderWithItems.OrderItems);
                    orderDtos.Add(orderDto);
                }

                return new AppPaginatedResponse<OrderReadDto>(orderDtos, pagination, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user orders for userId {UserId}", userId);
                return new AppPaginatedResponse<OrderReadDto>(
                    Enumerable.Empty<OrderReadDto>(),
                    new Pagination { CurrentPage = page, PageSize = pageSize, TotalItems = 0, TotalPages = 0 },
                    (int)HttpStatusCode.InternalServerError,
                    errors: new List<string> { "An error occurred while retrieving orders." });
            }
        }

        public async Task<AppResponse<OrderReadDto>> GetOrderByIdAsync(int orderId, int userId)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithItemsAsync(orderId, asNoTracking: true);
                if (order == null || order.UserId != userId)
                    return AppResponse<OrderReadDto>.ErrorResult(
                        new List<string> { $"Order with Id {orderId} not found." },
                        (int)HttpStatusCode.NotFound);

                var orderDto = _mapper.Map<OrderReadDto>(order);
                orderDto.OrderItems = _mapper.Map<List<OrderItemReadDto>>(order.OrderItems);
                return AppResponse<OrderReadDto>.SuccessResult(orderDto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId} for user {UserId}", orderId, userId);
                return AppResponse<OrderReadDto>.ErrorResult(
                    new List<string> { "An error occurred while retrieving the order." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<AppResponse<OrderReadDto>> CreateCashOrderAsync(int userId, CreateCashOrderDto dto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _createCashOrderValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                var cart = await _cartRepository.GetByUserIdAsync(userId, asNoTracking: false);
                if (cart == null || !cart.Any())
                    errors.Add("Cart is empty.");

                var user = await _userRepository.GetByIdAsync(userId, asNoTracking: true);
                if (user == null || !user.IsActive)
                    errors.Add("User not found or inactive.");

                if (errors.Any())
                    return AppResponse<OrderReadDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);

                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var cartItem in cart)
                {
                    var product = await _productRepository.GetByIdAsync(cartItem.ProductId, asNoTracking: true);
                    if (product == null || !product.IsActive)
                    {
                        errors.Add($"Product '{product?.Name ?? cartItem.ProductId.ToString()}' is not available.");
                        continue;
                    }

                    if (product.Stock < cartItem.Quantity)
                    {
                        errors.Add($"Insufficient stock for product '{product.Name}'. Available: {product.Stock}, Requested: {cartItem.Quantity}");
                        continue;
                    }

                    orderItems.Add(new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice
                    });
                    totalAmount += cartItem.Quantity * cartItem.UnitPrice;
                }

                if (errors.Any())
                    return AppResponse<OrderReadDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);

                var orderNumber = await _orderRepository.GenerateOrderNumberAsync();
                var order = new Order
                {
                    UserId = userId,
                    OrderNumber = orderNumber,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending,
                    PaymentMethod = PaymentMethod.CashOnDelivery,
                    PaymentStatus = PaymentStatus.Pending,
                    ShippingAddress = dto.ShippingAddress.Trim(),
                    Notes = dto.Notes?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OrderItems = orderItems
                };

                await _orderRepository.AddAsync(order, saveChanges: true);

                foreach (var orderItem in orderItems)
                {
                    var product = await _productRepository.GetByIdAsync(orderItem.ProductId, asNoTracking: false);
                    if (product != null)
                    {
                        product.Stock -= orderItem.Quantity;
                        await _productRepository.UpdateAsync(product, saveChanges: false);
                    }
                }

                await _cartRepository.ClearCartAsync(userId);
                await _cartRepository.SaveChangesAsync();

                var createdOrder = await _orderRepository.GetOrderWithItemsAsync(order.Id, asNoTracking: true);
                var orderDto = _mapper.Map<OrderReadDto>(createdOrder);
                orderDto.OrderItems = _mapper.Map<List<OrderItemReadDto>>(createdOrder!.OrderItems);

                return AppResponse<OrderReadDto>.SuccessResult(orderDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cash order for user {UserId}", userId);
                return AppResponse<OrderReadDto>.ErrorResult(
                    new List<string> { "An error occurred while creating the order." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        
    }
}