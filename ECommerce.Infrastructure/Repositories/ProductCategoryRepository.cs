using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly ECommerceDbContext _context;

        public ProductCategoryRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task ReplaceCategoriesAsync(int productId, List<int> categoryIds, bool saveChanges = true)
        {
            await RemoveAllByProductIdAsync(productId, false);
            await AddRangeAsync(productId, categoryIds, false);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task AddCategoriesAsync(int productId, List<int> categoryIds, bool saveChanges = true)
        {
            await AddRangeAsync(productId, categoryIds, false);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

       
        public async Task<IEnumerable<ProductCategory>> GetByProductIdAsync(int productId, bool asNoTracking = true)
        {
            var query = _context.ProductCategories
                .Include(pc => pc.Category)
                .Where(pc => pc.ProductId == productId);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

      
        public async Task AddAsync(ProductCategory productCategory, bool saveChanges = true)
        {
            await _context.ProductCategories.AddAsync(productCategory);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int productId,int categoryId, bool saveChanges = true)
        {
            var productCategory = await _context.ProductCategories
                .FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.CategoryId == categoryId);
            _context.ProductCategories.Remove(productCategory);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

      
        public async Task DeleteByProductIdAsync(int productId, bool saveChanges = true)
        {
            await RemoveAllByProductIdAsync(productId, false);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();


        // ---------- private helpers ----------

        private async Task RemoveAllByProductIdAsync(int productId, bool saveChanges = true)
        {
            var existing = _context.ProductCategories
                .Where(pc => pc.ProductId == productId);

            _context.ProductCategories.RemoveRange(existing);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        private async Task AddRangeAsync(int productId, List<int> categoryIds, bool saveChanges = true)
        {
            var newAssignments = categoryIds.Select(cid => new ProductCategory
            {
                ProductId = productId,
                CategoryId = cid,
                CreatedAt = DateTime.UtcNow
            });

            await _context.ProductCategories.AddRangeAsync(newAssignments);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task<ProductCategory> GetByProductIdAndCategoryId(int productId, int categoryId, bool asNoTracking = true)
        {
            return await _context.ProductCategories.FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.CategoryId == categoryId);
        }
    }
}
