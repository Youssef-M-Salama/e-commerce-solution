using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    public interface IProductRepository
    {
        /// <summary>
        /// Retrieves all products without any filters.
        /// Returns an empty collection if no products are found.
        /// </summary>
        Task<IEnumerable<Product>> GetAllAsync();

        /// <summary>
        /// Retrieves products with optional filters and pagination.
        /// Returns an empty collection if no products are found.
        /// </summary>
        /// <param name="page">Page number (default = 1).</param>
        /// <param name="search">Search term for product name (default = "").</param>
        /// <param name="categoryId">Filter by category ID (optional).</param>
        /// <param name="brandId">Filter by brand ID (optional).</param>
        /// <param name="pageSize">Number of items per page (default = 10).</param>
        ///  /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries.
        /// </param>
        Task<IEnumerable<Product>> GetAllAsync(
            int page = 1,
            string search = "",
            int? categoryId = null,
            int? brandId = null,
            int pageSize = 10,
            bool asNoTracking = false
        );

        /// <summary>
        /// Retrieves all Products optionally filtered by a search term.
        /// Returns an empty collection if no Products match the search.
        /// </summary>
        /// <param name="search">
        /// Search term for Products name.  
        /// If null or empty, all Products will be returned without filtering.
        /// </param>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries.
        /// </param>
        /// <returns>A collection of Products matching the filter.</returns>
        public Task<IEnumerable<Product>> GetAllAsync(string search, bool asNoTracking = false);

        // <summary>
        /// Retrieves a single product by its ID, or <c>null</c> if not found.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries.
        /// </param>
        Task<Product?> GetByIdAsync(int id, bool asNoTracking = false);

        /// <summary>
        /// Adds a new product to the database.
        /// </summary>
        /// <param name="product">The product to add.</param>
        /// <param name="saveChanges">
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default is true).
        /// </param>
        Task AddAsync(Product product, bool saveChanges = true);

        /// <summary>
        /// Updates an existing product in the database.
        /// </summary>
        /// <param name="product">The product to update.</param>
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default is true).
        /// </param>
        Task UpdateAsync(Product product, bool saveChanges = true);

        /// <summary>
        /// Deletes a product from the database.
        /// </summary>
        /// <param name="product">The product to delete.</param>
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default is true).
        /// </param>
        Task DeleteAsync(Product product, bool saveChanges = true);
        /// <summary>
        /// Persists all pending changes in the repository.
        /// </summary>
        Task SaveChangesAsync();
    }
}
