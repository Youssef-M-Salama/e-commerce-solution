using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    public interface ISubCategoryRepository
    {
        ///// <summary>
        ///// Retrieves all subcategories without any filter.
        ///// Returns an empty collection if no products are found.
        ///// </summary>
        ///// <param name="asNoTracking">
        ///// If true, disables change tracking for performance in read-only queries.
        ///// </param>

        //Task<IEnumerable<SubCategory>> GetAllAsync(bool asNoTracking = false);

        ///// <summary>
        ///// Retrieves all subcategories for a specific category.
        ///// Returns an empty collection if no products are found.
        ///// </summary>
        ///// <param name="categoryId">The ID of the category.</param>
        ///// <param name="asNoTracking">
        ///// If true, disables change tracking for performance in read-only queries.
        ///// </param>

        //Task<IEnumerable<SubCategory>> GetAllAsync(int categoryId, bool asNoTracking = false);

        ///// <summary>
        ///// Retrieves subcategories with optional search, category filter, and pagination.
        ///// Returns an empty collection if no products are found.
        ///// </summary>
        ///// <param name="page">Page number (default = 1).</param>
        ///// <param name="search">Search term for subcategory name (default = "").</param>
        ///// <param name="categoryId">Filter by category ID (optional).</param>
        ///// <param name="pageSize">Number of items per page (default = 10).</param>
        ///// <param name="asNoTracking">
        ///// If true, disables change tracking for performance in read-only queries.
        ///// </param>

        //Task<IEnumerable<SubCategory>> GetAllAsync(
        //    int page = 1,
        //    string search = "",
        //    int? categoryId = null,
        //    int pageSize = 10,
        //    bool asNoTracking = false);

        ///// <summary>
        ///// Retrieves a single subcategory by its ID, or <c>null</c> if not found.
        ///// </summary>
        ///// <param name="id">The ID of the subcategory.</param>
        ///// <param name="asNoTracking">
        ///// If true, disables change tracking for performance in read-only queries.
        ///// </param>

        //Task<SubCategory?> GetByIdAsync(int id, bool asNoTracking = false);

        ///// <summary>
        ///// Adds a new subcategory.
        ///// </summary>
        ///// <param name="subCategory">The subcategory to add.</param>
        ///// <param name="asNoTracking">
        ///// If true, disables change tracking for performance in read-only queries.
        ///// </param>

        //Task AddAsync(SubCategory subCategory, bool saveChanges = true);

        ///// <summary>
        ///// Updates an existing subcategory.
        ///// </summary>
        ///// <param name="subCategory">The subcategory to update.</param>
        ///// <param name="saveChanges">
        ///// Whether to immediately save changes to the database (default is true).
        ///// </param>
        //Task UpdateAsync(SubCategory subCategory, bool saveChanges = true);

        ///// <summary>
        ///// Deletes a subcategory.
        ///// </summary>
        ///// <param name="subCategory">The subcategory to delete.</param>
        ///// <param name="saveChanges">
        ///// Whether to immediately save changes to the database (default is true).
        ///// </param>
        //Task DeleteAsync(SubCategory subCategory, bool saveChanges = true);

        ///// <summary>
        ///// Saves all pending changes in the context.
        ///// </summary>
        //Task SaveChangesAsync();
    }
}
