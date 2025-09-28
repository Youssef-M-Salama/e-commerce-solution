using ECommerce.Application.DTOs.Cart;
using FluentValidation;

namespace ECommerce.Application.Validators.Cart
{
    public class CartItemCreateDtoValidator : AbstractValidator<CartItemCreateDto>
    {
        public CartItemCreateDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than zero.");
        }
    }

    public class CartItemUpdateDtoValidator : AbstractValidator<CartItemUpdateDto>
    {
        public CartItemUpdateDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than zero.");
        }
    }
}
