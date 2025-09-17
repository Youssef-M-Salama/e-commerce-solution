using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    public interface ICategoryRepository
    {
        /// <summary>
        /// Retrieves all categories without any filters.
        /// Returns an empty collection if no categories are found.
        /// </summary>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries (default = true).
        /// </param>
        Task<IEnumerable<Category>> GetAllAsync(bool asNoTracking = true);

        /// <summary>
        /// Retrieves categories with optional search and pagination.
        /// Returns an empty collection if no categories are found.
        /// </summary>
        /// <param name="page">Page number (default = 1).</param>
        /// <param name="search">Search term for category name (default = "").</param>
        /// <param name="pageSize">Number of items per page (default = 10).</param>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries (default = true).
        /// </param>
        Task<IEnumerable<Category>> GetAllAsync(
            int page = 1,
            string search = "",
            int pageSize = 10,
            bool asNoTracking = true
        );

        /// <summary>
        /// Retrieves categories filtered by a search term.
        /// Returns all categories if search is null or empty.
        /// </summary>
        /// <param name="search">Search term for category name.</param>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries (default = false).
        /// </param>
        Task<IEnumerable<Category>> GetAllAsync(string search, bool asNoTracking = false);

        /// <summary>
        /// Retrieves a single category by its ID, or <c>null</c> if not found.
        /// </summary>
        /// <param name="id">The ID of the category.</param>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries (default = true).
        /// </param>
        Task<Category?> GetByIdAsync(int id, bool asNoTracking = true);

        /// <summary>
        /// Retrieves a category with its immediate children (subcategories).
        /// Returns <c>null</c> if the category is not found.
        /// </summary>
        /// <param name="id">The ID of the category.</param>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries (default = true).
        /// </param>
        Task<Category?> GetWithChildrenAsync(int id, bool asNoTracking = true);

        /// <summary>
        /// Retrieves immediate children (subcategories) of a given parent category.
        /// Returns an empty collection if no children are found.
        /// </summary>
        /// <param name="parentId">The ID of the parent category.</param>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries (default = true).
        /// </param>
        Task<IEnumerable<Category>> GetChildrenAsync(int parentId, bool asNoTracking = true);

        /// <summary>
        /// Adds a new category to the database.
        /// </summary>
        /// <param name="category">The category to add.</param>
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default = true).
        /// </param>
        Task AddAsync(Category category, bool saveChanges = true);

        /// <summary>
        /// Updates an existing category in the database.
        /// </summary>
        /// <param name="category">The category to update.</param>
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default = true).
        /// </param>
        Task UpdateAsync(Category category, bool saveChanges = true);

        /// <summary>
        /// Deletes a category from the database.
        /// </summary>
        /// <param name="category">The category to delete.</param>
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default = true).
        /// </param>
        Task DeleteAsync(Category category, bool saveChanges = true);

        /// <summary>
        /// Persists all pending changes in the repository.
        /// </summary>
        Task SaveChangesAsync();
    }
}
