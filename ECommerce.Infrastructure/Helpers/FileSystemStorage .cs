
using ECommerce.Domain.Helpers;

namespace ECommerce.Infrastructure.Helpers
{
    public class FileSystemStorage : IFileStorage
    {
        public async Task<string> SaveFileAsync(Stream stream, string extension, string subFolder)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", subFolder);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await stream.CopyToAsync(fileStream);
            }

            return $"/images/{subFolder}/{fileName}";
        }

        public bool DeleteFile(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return false;

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }

        public async Task<string> UpdateFileAsync(Stream stream, string extension, string subFolder, string? oldRelativePath)
        {
            if (!string.IsNullOrWhiteSpace(oldRelativePath))
                DeleteFile(oldRelativePath);

            return await SaveFileAsync(stream, extension, subFolder);
        }
    }
}