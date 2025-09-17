using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    public interface IBrandRepository
    {
        /// <summary>
        /// Retrieves all brands.
        /// Returns an empty collection if no products are found.
        /// </summary>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries.
        /// </param>
        Task<IEnumerable<Brand>> GetAllAsync(bool asNoTracking = false);

        /// <summary>
        /// Retrieves brands with optional paging and search by name.
        /// Returns an empty collection if no products are found.
        /// </summary>
        /// <param name="page">Page number (default is 1).</param>
        /// <param name="search">Search term for brand name.</param>
        /// <param name="pageSize">Number of items per page (default is 10).</param>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries.
        /// </param>
        Task<IEnumerable<Brand>> GetAllAsync(int page = 1, string search = "", int pageSize = 10, bool asNoTracking = false);

        /// <summary>
        /// Retrieves all Brands optionally filtered by a search term.
        /// Returns an empty collection if no Brands match the search.
        /// </summary>
        /// <param name="search">
        /// Search term for Brands name.  
        /// If null or empty, all Brands will be returned without filtering.
        /// </param>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries.
        /// </param>
        /// <returns>A collection of Brands matching the filter.</returns>
        public Task<IEnumerable<Brand>> GetAllAsync(string search, bool asNoTracking = false);

        /// <summary>
        /// Retrieves a single brand by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the brand.</param>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries.
        /// </param>
        /// <returns>
        /// The brand with the specified ID, or <c>null</c> if not found.
        /// </returns>
        Task<Brand?> GetByIdAsync(int id, bool asNoTracking = false);

        /// <summary>
        /// Adds a new brand to the repository.
        /// </summary>
        /// <param name="brand">The brand to add.</param>
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default is true).
        /// </param>
        Task AddAsync(Brand brand, bool saveChanges = true);

        /// <summary>
        /// Updates an existing brand.
        /// </summary>
        /// <param name="brand">The brand to update.</param>
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default is true).
        /// </param>
        Task UpdateAsync(Brand brand, bool saveChanges = true);

        /// <summary>
        /// Deletes a brand from the repository.
        /// </summary>
        /// <param name="brand">The brand to delete.</param>
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default is true).
        /// </param>
        Task DeleteAsync(Brand brand, bool saveChanges = true);

        /// <summary>
        /// Persists all pending changes in the context to the database.
        /// </summary>
        Task SaveChangesAsync();
    }
}
