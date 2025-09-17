using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    /// <summary>
    /// Repository interface for managing product images.
    /// Provides CRUD operations and specialized queries for product images.
    /// </summary>
    public interface IProductImageRepository
    {
        /// <summary>
        /// Adds a new product image.
        /// </summary>
        /// <param name="image">The product image entity to add.</param>
        /// <param name="saveChanges">Whether to immediately save changes to the database.</param>
        Task AddAsync(ProductImage image, bool saveChanges = true);

        /// <summary>
        /// Deletes a product image.
        /// </summary>
        /// <param name="image">The product image entity to delete.</param>
        /// <param name="saveChanges">Whether to immediately save changes to the database.</param>
        Task DeleteAsync(ProductImage image, bool saveChanges = true);

        /// <summary>
        /// Updates a product image.
        /// </summary>
        /// <param name="image">The product image entity to update.</param>
        /// <param name="saveChanges">Whether to immediately save changes to the database.</param>
        Task UpdateAsync(ProductImage image, bool saveChanges = true);

        /// <summary>
        /// Retrieves all images for a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="asNoTracking">Whether to disable EF tracking for performance in read-only queries.</param>
        Task<IEnumerable<ProductImage>> GetAllByProductIdAsync(int productId, bool asNoTracking = false);

        /// <summary>
        /// Retrieves paginated images for a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="page">The page number (default = 1).</param>
        /// <param name="pageSize">The number of items per page (default = 10).</param>
        /// <param name="asNoTracking">Whether to disable EF tracking for performance in read-only queries.</param>
        Task<IEnumerable<ProductImage>> GetAllByProductIdAsync(int productId, int page = 1, int pageSize = 10, bool asNoTracking = false);

        /// <summary>
        /// Retrieves the main image of a product, or the first one if none is marked as main.
        /// Returns null if no images exist.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="asNoTracking">Whether to disable EF tracking for performance in read-only queries.</param>
        Task<ProductImage?> GetMainOrFirstAsync(int productId, bool asNoTracking = false);

        /// <summary>
        /// Retrieves a product image by its ID.
        /// </summary>
        /// <param name="id">The ID of the image.</param>
        /// <param name="asNoTracking">Whether to disable EF tracking for performance in read-only queries.</param>
        Task<ProductImage?> GetByIdAsync(int id, bool asNoTracking = false);

        /// <summary>
        /// Sets a specific image as the main image for a product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="imageId">The ID of the image to set as main.</param>
        /// <param name="saveChanges">Whether to immediately save changes to the database.</param>
        Task SetMainAsync(int productId, int imageId, bool saveChanges = true);

        /// <summary>
        /// Checks if a display order value exists for a given product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="displayOrder">The display order to check.</param>
        /// <param name="excludeId">Optional image ID to exclude from the check (useful for updates).</param>
        Task<bool> DoesDisplayOrderExistAsync(int productId, int displayOrder, int? excludeId = null);

        /// <summary>
        /// Checks if a main image already exists for a given product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="excludeId">Optional image ID to exclude from the check (useful for updates).</param>
        Task<bool> DoesMainImageExistAsync(int productId, int? excludeId = null);

        /// <summary>
        /// Persists all tracked changes to the database.
        /// Useful when multiple operations are performed with saveChanges = false.
        /// </summary>
        Task SaveChangesAsync();
    }
}
