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
    public class BrandDtoValidator:AbstractValidator<BrandDto>
    {
        public BrandDtoValidator() {
            RuleFor(b => b.Name)
                .NotEmpty().WithMessage("Brand name is required.")
                .MaximumLength(255).WithMessage("Brand name cannot exceed 255 characters.")
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Brand name cannot be all whitespace.");



            RuleFor(b => b.Description)
                .MaximumLength(1000).When(b => !string.IsNullOrEmpty(b.Description))
                .WithMessage("Description cannot exceed 1000 characters.");


            RuleFor(b => b.IsActive)
                .NotNull().WithMessage("IsActive must be specified (true or false).");



            RuleFor(b => b.ImageFile)
                 .Must(file => file == null || ImageUploadSettings.AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                 .WithMessage($"Invalid file type. Allowed: {string.Join(" ", ImageUploadSettings.AllowedExtensions)}")
                 .Must(file => file == null || file.Length <= ImageUploadSettings.MaxFileSizeInMB * 1024 * 1024)
                 .WithMessage($"File size cannot exceed {ImageUploadSettings.MaxFileSizeInMB} MB.");
        }

    }
}
