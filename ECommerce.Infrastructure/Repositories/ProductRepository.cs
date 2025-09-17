using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ECommerceDbContext _context;

        public ProductRepository(ECommerceDbContext context)
        {
            _context = context;
         }

        public async Task AddAsync(Product product, bool saveChanges = true)
        {
            await _context.Products.AddAsync(product);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product, bool saveChanges = true)
        {
            _context.Products.Update(product);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product, bool saveChanges = true)
        {
            _context.Products.Remove(product);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task<Product?> GetByIdAsync(int id, bool asNoTracking = false)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .AsQueryable();

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Brand)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllAsync(
            int page = 1,
            string search = "",
            int? categoryId = null,
            int? brandId = null,
            int pageSize = 10,
            bool asNoTracking = false)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => EF.Functions.Like(p.Name, $"%{search}%"));

            if (categoryId.HasValue)
                query = query.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId));


         

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetAllAsync(
            string search = "",
            bool asNoTracking = false)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => EF.Functions.Like(p.Name, $"%{search}%"));

            if (asNoTracking)
                query = query.AsNoTracking();
            return await query
                .OrderBy(p => p.Name).ToListAsync();
               
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
