using ECommerce.API.Admin.Application.DTOs;
using ECommerce.Application.Shared;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Validators
{
    public class ProductImageValidator : AbstractValidator<ProductImageDto>
    {
        public ProductImageValidator()
        {
            RuleFor(pi => pi.ImageFile)
                 .NotNull().WithMessage("Image file is required.")
                 .Must(file => ImageUploadSettings.AllowedExtensions.Contains(Path.GetExtension(file?.FileName??"").ToLowerInvariant()))
                 .WithMessage($"Invalid file type. Allowed: {string.Join(" ", ImageUploadSettings.AllowedExtensions)}")
                 .Must(file => file?.Length <= ImageUploadSettings.MaxFileSizeInMB * 1024 * 1024)
                 .WithMessage($"File size cannot exceed {ImageUploadSettings.MaxFileSizeInMB} MB.");

            RuleFor(pi=>pi.DisplayOrder)
                .NotNull() .WithMessage("Display order is required.")
                .GreaterThan(0).WithMessage("Display order must be greater than zero.");
        }
    }
}
