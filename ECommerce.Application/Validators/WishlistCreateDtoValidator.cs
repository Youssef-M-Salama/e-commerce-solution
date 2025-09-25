using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators
{
    // ---------------- Wishlist ----------------
    public class WishlistCreateDtoValidator : AbstractValidator<WishlistCreateDto>
    {
        public WishlistCreateDtoValidator()
        {
            RuleFor(w => w.UserId)
                .NotNull().WithMessage("UserId is required.")
                .GreaterThan(0).WithMessage("UserId must be greater than 0.");
        }
    }

    public class WishlistDeleteDtoValidator : AbstractValidator<WishlistDeleteDto>
    {
        public WishlistDeleteDtoValidator()
        {
            RuleFor(w => w.UserId)
                .NotNull().WithMessage("UserId is required.")
                .GreaterThan(0).WithMessage("UserId must be greater than 0.");
        }
    }

    // ---------------- Wishlist Item ----------------
    public class WishlistItemCreateDtoValidator : AbstractValidator<WishlistItemCreateDto>
    {
        public WishlistItemCreateDtoValidator()
        {
            RuleFor(w => w.WishlistId)
                .NotNull().WithMessage("WishlistId is required.")
                .GreaterThan(0).WithMessage("WishlistId must be greater than 0.");

            RuleFor(w => w.ProductId)
                .NotNull().WithMessage("ProductId is required.")
                .GreaterThan(0).WithMessage("ProductId must be greater than 0.");
        }
    }

    public class WishlistItemDeleteDtoValidator : AbstractValidator<WishlistItemDeleteDto>
    {
        public WishlistItemDeleteDtoValidator()
        {
            RuleFor(w => w.WishlistId)
                .NotNull().WithMessage("WishlistId is required.")
                .GreaterThan(0).WithMessage("WishlistId must be greater than 0.");

            RuleFor(w => w.ProductId)
                .NotNull().WithMessage("ProductId is required.")
                .GreaterThan(0).WithMessage("ProductId must be greater than 0.");
        }
    }
}
