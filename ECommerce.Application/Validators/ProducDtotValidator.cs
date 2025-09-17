using ECommerce.API.Admin.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Validators
{
    public class ProducDtotValidator: AbstractValidator<ProductDto>
    {
        public ProducDtotValidator() { 
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(255).WithMessage("Product name must not exceed 255 characters.");

            RuleFor(p => p.Description)
                .MaximumLength(2000).When(p=>!string.IsNullOrEmpty(p.Description))
                .WithMessage("Description cannot exceed 2000 characters.");

            RuleFor(p => p.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be a non-negative value.")
                .PrecisionScale(18,2,true).WithMessage("Price must have up to 18 digits in total and 2 decimal places.");
            RuleFor(p => p.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock must be a non-negative integer.");

            RuleFor(p => p.IsActive)
                .NotNull().WithMessage("IsActive must be specified (true or false).");
          
        }
    }
}
