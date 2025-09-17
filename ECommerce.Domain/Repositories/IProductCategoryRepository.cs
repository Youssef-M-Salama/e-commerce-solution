using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    public interface IProductCategoryRepository
    {
        /// <summary>
        /// Replace all existing categories for a product with the given ones.
        /// Old categories are removed before adding the new list.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="categoryIds">List of category IDs to replace.</param>
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default is true).
        /// </param>
        Task ReplaceCategoriesAsync(int productId, List<int> categoryIds, bool saveChanges = true);
         /// <summary>
         /// Add the given categories to a product without removing existing ones.
         /// </summary>
         /// <param name="productId">The ID of the product.</param>
         /// <param name="categoryIds">List of category IDs to assign.</param>
         /// <param name="saveChanges">
         /// Whether to immediately save changes to the database (default is true).
         /// </param>
         Task AddCategoriesAsync(int productId, List<int> categoryIds, bool saveChanges = true);

        /// <summary>
        /// Retrieves all categories assigned to a specific product.
        /// Returns an empty collection if no products are found.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="asNoTracking">
        /// If true, disables change tracking for performance in read-only queries.
        /// </param>
        Task<IEnumerable<ProductCategory>> GetByProductIdAsync(int productId, bool asNoTracking = true);
        Task<ProductCategory>GetByProductIdAndCategoryId(int productId,int categoryId, bool asNoTracking = true);

        /// <summary>
        /// Adds a single product-category assignment.

        /// </summary>
        /// <param name="productCategory">The product-category assignment to add.</param>
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default is true).
        /// </param>
        Task AddAsync(ProductCategory productCategory, bool saveChanges = true);

        /// <summary>
        /// Deletes a specific product-category assignment.
        /// </summary>
        /// <param name="productId">The product ID of the assignment to delete.</param>
        /// <param name="categoryId">The category ID of the assignment to delete.</param>
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default is true).
        /// </param>
        Task DeleteAsync(int productId, int categoryId, bool saveChanges = true);

        /// <summary>
        /// Deletes all category assignments for a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="saveChanges">
        /// Whether to immediately save changes to the database (default is true).
        /// </param>
        Task DeleteByProductIdAsync(int productId, bool saveChanges = true);

        /// <summary>
        /// Persists all pending changes in the repository.
        /// </summary>
        Task SaveChangesAsync();
    }
}
