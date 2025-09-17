using ECommerce.API.Admin.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Validators
{
    public class ProductCategoryDtoValidator:AbstractValidator<ProductCategoryDto>
    {
        public ProductCategoryDtoValidator() {
            RuleFor(pc => pc.CategoryId)
                .NotNull().WithMessage("CategoryId is required.");
            RuleFor(pc => pc.ProductId)
                .NotNull().WithMessage("ProductId is required.");
        }
    }
}
