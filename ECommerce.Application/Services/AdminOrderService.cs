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

namespace ECommerce.API.Admin.Application.Services
{
    public class AdminOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IValidator<CreateOrderDto> _createOrderValidator;
        private readonly IValidator<UpdateOrderDto> _updateOrderValidator;
        private readonly IValidator<UpdateOrderStatusDto> _updateOrderStatusValidator;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminOrderService> _logger;

        public AdminOrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IValidator<CreateOrderDto> createOrderValidator,
            IValidator<UpdateOrderDto> updateOrderValidator,
            IValidator<UpdateOrderStatusDto> updateOrderStatusValidator,
            IMapper mapper,
            ILogger<AdminOrderService> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _createOrderValidator = createOrderValidator ?? throw new ArgumentNullException(nameof(createOrderValidator));
            _updateOrderValidator = updateOrderValidator ?? throw new ArgumentNullException(nameof(updateOrderValidator));
            _updateOrderStatusValidator = updateOrderStatusValidator ?? throw new ArgumentNullException(nameof(updateOrderStatusValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AppPaginatedResponse<OrderReadDto>> GetAllAsync(int page = 1, string search = "", int pageSize = 10)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Max(1, pageSize);

                var allOrders = await _orderRepository.GetAllAsync(search, asNoTracking: true);
                var totalOrders = allOrders.Count();

                var pagination = (totalOrders, page, pageSize).BuildPagination();

                if (totalOrders == 0)
                    return PaginationExtensions.EmptyPageResult<OrderReadDto>(pageSize);

                if (page > pagination.TotalPages && pagination.TotalPages > 0)
                    return pagination.NotFoundPageResult<OrderReadDto>();

                var pagedOrders = await _orderRepository.GetAllAsync(page, search ?? string.Empty, pageSize, asNoTracking: true);
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
                _logger.LogError(ex, "Error retrieving orders.");
                return new AppPaginatedResponse<OrderReadDto>(
                    Enumerable.Empty<OrderReadDto>(),
                    new Pagination { CurrentPage = page, PageSize = pageSize, TotalItems = 0, TotalPages = 0 },
                    (int)HttpStatusCode.InternalServerError,
                    errors: new List<string> { "An error occurred while retrieving orders." });
            }
        }

        public async Task<AppResponse<OrderReadDto>> GetByIdAsync(int id)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithItemsAsync(id, asNoTracking: true);
                if (order == null)
                    return AppResponse<OrderReadDto>.ErrorResult(
                        new List<string> { $"Order with Id {id} not found." },
                        (int)HttpStatusCode.NotFound);

                var orderDto = _mapper.Map<OrderReadDto>(order);
                orderDto.OrderItems = _mapper.Map<List<OrderItemReadDto>>(order.OrderItems);
                return AppResponse<OrderReadDto>.SuccessResult(orderDto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with ID {OrderId}", id);
                return AppResponse<OrderReadDto>.ErrorResult(
                    new List<string> { "An error occurred while retrieving the order." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<AppResponse<OrderReadDto>> CreateAsync(CreateOrderDto dto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _createOrderValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                var user = await _userRepository.GetByIdAsync(dto.UserId, asNoTracking: true);
                if (user == null || !user.IsActive)
                    errors.Add("User not found or inactive.");

                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var itemDto in dto.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(itemDto.ProductId, asNoTracking: true);
                    if (product == null || !product.IsActive)
                    {
                        errors.Add($"Product with Id {itemDto.ProductId} not found or inactive.");
                        continue;
                    }

                    if (product.Stock < itemDto.Quantity)
                    {
                        errors.Add($"Insufficient stock for product '{product.Name}'. Available: {product.Stock}, Requested: {itemDto.Quantity}");
                        continue;
                    }

                    orderItems.Add(new OrderItem
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice
                    });
                    totalAmount += itemDto.Quantity * itemDto.UnitPrice;
                }

                if (errors.Any())
                    return AppResponse<OrderReadDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);

                var orderNumber = await _orderRepository.GenerateOrderNumberAsync();
                var order = new Order
                {
                    UserId = dto.UserId,
                    OrderNumber = orderNumber,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending,
                    PaymentMethod = dto.PaymentMethod,
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
                await _productRepository.SaveChangesAsync();

                var createdOrder = await _orderRepository.GetOrderWithItemsAsync(order.Id, asNoTracking: true);
                var orderDto = _mapper.Map<OrderReadDto>(createdOrder);
                orderDto.OrderItems = _mapper.Map<List<OrderItemReadDto>>(createdOrder!.OrderItems);

                return AppResponse<OrderReadDto>.SuccessResult(orderDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", dto?.UserId);
                return AppResponse<OrderReadDto>.ErrorResult(
                    new List<string> { "An error occurred while creating the order." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<AppResponse<OrderReadDto>> UpdateAsync(int id, UpdateOrderDto dto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _updateOrderValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                var order = await _orderRepository.GetByIdAsync(id, asNoTracking: false);
                if (order == null)
                    errors.Add($"Order with Id {id} not found.");

                if (errors.Any())
                    return AppResponse<OrderReadDto>.ErrorResult(errors, (int)HttpStatusCode.NotFound);

                order!.Status = dto.Status;
                order.PaymentStatus = dto.PaymentStatus;
                order.ShippingAddress = dto.ShippingAddress.Trim();
                order.Notes = dto.Notes?.Trim();
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(order, saveChanges: true);

                var updatedOrder = await _orderRepository.GetOrderWithItemsAsync(order.Id, asNoTracking: true);
                var orderDto = _mapper.Map<OrderReadDto>(updatedOrder);
                orderDto.OrderItems = _mapper.Map<List<OrderItemReadDto>>(updatedOrder!.OrderItems);

                return AppResponse<OrderReadDto>.SuccessResult(orderDto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", id);
                return AppResponse<OrderReadDto>.ErrorResult(
                    new List<string> { "An error occurred while updating the order." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<AppResponse<object>> UpdateStatusAsync(int id, UpdateOrderStatusDto dto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _updateOrderStatusValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                var order = await _orderRepository.GetByIdAsync(id, asNoTracking: false);
                if (order == null)
                    errors.Add($"Order with Id {id} not found.");

                if (errors.Any())
                {
                    var statusCode = errors.Any(e => e.Contains("not found"))
                        ? (int)HttpStatusCode.NotFound
                        : (int)HttpStatusCode.BadRequest;
                    return AppResponse<object>.ErrorResult(errors, statusCode);
                }

                order!.Status = dto.Status;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(order, saveChanges: true);

                return AppResponse<object>.SuccessResult(null, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status {OrderId}", id);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while updating the order status." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<AppResponse<object>> DeleteAsync(int id)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithItemsAsync(id, asNoTracking: false);
                if (order == null)
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"Order with Id {id} not found." },
                        (int)HttpStatusCode.NotFound);

                if (order.Status != OrderStatus.Pending)
                    return AppResponse<object>.ErrorResult(
                        new List<string> { "Only pending orders can be deleted." },
                        (int)HttpStatusCode.BadRequest);

                foreach (var orderItem in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(orderItem.ProductId, asNoTracking: false);
                    if (product != null)
                    {
                        product.Stock += orderItem.Quantity;
                        await _productRepository.UpdateAsync(product, saveChanges: false);
                    }
                }

                await _orderRepository.DeleteAsync(order, saveChanges: false);
                await _orderRepository.SaveChangesAsync();

                return AppResponse<object>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order {OrderId}", id);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while deleting the order." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}