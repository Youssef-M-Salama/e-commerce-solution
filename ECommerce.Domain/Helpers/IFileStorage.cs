namespace ECommerce.Domain.Helpers
{
    public interface IFileStorage
    {
        Task<string> SaveFileAsync(Stream stream, string extension, string subFolder);
        Task<string> UpdateFileAsync(Stream stream, string extension, string subFolder, string? oldRelativePath);
        bool DeleteFile(string relativePath);
    }
}
