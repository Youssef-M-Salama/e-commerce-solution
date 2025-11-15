using ECommerce.API.Admin.Application.DTOs;
using ECommerce.Domain.Enums;
using FluentValidation;

namespace ECommerce.API.Admin.Application.Validators
{
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required")
                .GreaterThan(0).WithMessage("UserId must be greater than 0");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required")
                .Must(pm => Enum.IsDefined(typeof(PaymentMethod), pm))
                .WithMessage("Invalid payment method. Must be CashOnDelivery, CreditCard, or PayPal");

            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Shipping address is required")
                .MaximumLength(1000).WithMessage("Shipping address cannot exceed 1000 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            RuleFor(x => x.OrderItems)
                .NotEmpty().WithMessage("Order must have at least one item")
                .Must(items => items != null && items.Count > 0).WithMessage("Order must have at least one item");

            RuleForEach(x => x.OrderItems).SetValidator(new CreateOrderItemDtoValidator());
        }
    }

    public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("ProductId is required")
                .GreaterThan(0).WithMessage("ProductId must be greater than 0");

            RuleFor(x => x.Quantity)
                .NotEmpty().WithMessage("Quantity is required")
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000");

            RuleFor(x => x.UnitPrice)
                .NotEmpty().WithMessage("Unit price is required")
                .GreaterThan(0).WithMessage("Unit price must be greater than 0")
                .PrecisionScale(18, 2,true).WithMessage("Unit price can have maximum 2 decimal places");
        }
    }

    public class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
    {
        public UpdateOrderDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(s => Enum.IsDefined(typeof(OrderStatus), s))
                .WithMessage("Invalid order status. Must be Pending, Processing, Shipped, Delivered, or Cancelled");

            RuleFor(x => x.PaymentStatus)
                .NotEmpty().WithMessage("Payment status is required")
                .Must(ps => Enum.IsDefined(typeof(PaymentStatus), ps))
                .WithMessage("Invalid payment status. Must be Pending, Paid, Failed, or Refunded");

            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Shipping address is required")
                .MaximumLength(1000).WithMessage("Shipping address cannot exceed 1000 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }

    public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(s => Enum.IsDefined(typeof(OrderStatus), s))
                .WithMessage("Invalid order status. Must be Pending, Processing, Shipped, Delivered, or Cancelled");
        }
    }

    public class CreateCashOrderDtoValidator : AbstractValidator<CreateCashOrderDto>
    {
        public CreateCashOrderDtoValidator()
        {
            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Shipping address is required")
                .MaximumLength(1000).WithMessage("Shipping address cannot exceed 1000 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }

    public class CreateCheckoutOrderDtoValidator : AbstractValidator<CreateCheckoutOrderDto>
    {
        public CreateCheckoutOrderDtoValidator()
        {
            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Shipping address is required")
                .MaximumLength(1000).WithMessage("Shipping address cannot exceed 1000 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            RuleFor(x => x.SuccessUrl)
                .NotEmpty().WithMessage("Success URL is required")
                .Must(BeAValidUrl).WithMessage("Invalid success URL format");

            RuleFor(x => x.CancelUrl)
                .NotEmpty().WithMessage("Cancel URL is required")
                .Must(BeAValidUrl).WithMessage("Invalid cancel URL format");
        }

        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}