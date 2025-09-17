using ECommerce.Application.Shared;
using ECommerce.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
public static class FormFileExtensions
{
    public static async Task<string?> SaveImageAsync(this IFormFile? file, string subFolder, int maxFileSizeInMB = 5)
    {
        if (file == null || file.Length == 0) return null;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ImageUploadSettings.AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Invalid file type. Only image files are allowed.");

        var maxFileSize = maxFileSizeInMB * 1024 * 1024;
        if (file.Length > maxFileSize)
            throw new InvalidOperationException($"File size cannot exceed {maxFileSizeInMB} MB.");

        using var stream = file.OpenReadStream();
        return await FileHelper.SaveFileAsync(stream, extension, subFolder);
    }
}