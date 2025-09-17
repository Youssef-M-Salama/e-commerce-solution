namespace ECommerce.Infrastructure.Helpers
{
    public static class FileHelper
    {
        public static async Task<string> SaveFileAsync(Stream stream, string extension, string subFolder)
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

        public static bool DeleteFile(string relativePath)
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

        public static async Task<string> UpdateFileAsync(Stream stream, string extension, string subFolder, string? oldRelativePath)
        {
            // لو فيه ملف قديم امسحه
            if (!string.IsNullOrWhiteSpace(oldRelativePath))
                DeleteFile(oldRelativePath);

            // احفظ الملف الجديد
            return await SaveFileAsync(stream, extension, subFolder);
        }
    }
}
