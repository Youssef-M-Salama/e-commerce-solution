using ECommerce.API.Admin.Application.DTOs;
using ECommerce.Application.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace ECommerce.API.Admin.Application.Validators
{
    public class CategoryDtoValidator : AbstractValidator<CategoryDto>
    {
        public CategoryDtoValidator()
        {

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(255).WithMessage("Category name cannot exceed 255 characters.")
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Category name cannot be all whitespace.");



            RuleFor(x => x.Description)
                .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description cannot exceed 1000 characters.");


            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("IsActive must be specified (true or false).");


            RuleFor(x => x.ImageFile)
                 .Must(file => file == null || ImageUploadSettings.AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                 .WithMessage($"Invalid file type. Allowed: {string.Join(" ", ImageUploadSettings.AllowedExtensions)}")
                 .Must(file => file == null || file.Length <= ImageUploadSettings.MaxFileSizeInMB * 1024 * 1024)
                 .WithMessage($"File size cannot exceed {ImageUploadSettings.MaxFileSizeInMB} MB.");

        }
    }
}
