using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ECommerceDbContext _context;

        public CategoryRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(bool asNoTracking = true)
        {
            var query = _context.Categories.AsQueryable();
            if (asNoTracking) query = query.AsNoTracking();
            return await query.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllAsync(int page = 1, string search = "", int pageSize = 10, bool asNoTracking = true)
        {
            var query = _context.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => EF.Functions.Like(c.Name, $"%{search}%"));
            if (asNoTracking) query = query.AsNoTracking();

            return await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllAsync(string search, bool asNoTracking = false)
        {
            var query = _context.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => EF.Functions.Like(c.Name, $"%{search}%"));
            if (asNoTracking) query = query.AsNoTracking();
            return await query.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id, bool asNoTracking = true)
        {
            var query = _context.Categories.AsQueryable();
            if (asNoTracking) query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category?> GetWithChildrenAsync(int id, bool asNoTracking = true)
        {
            var query = _context.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.Id == id);

            if (asNoTracking) query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Category>> GetChildrenAsync(int parentId, bool asNoTracking = true)
        {
            if (_context?.Categories == null)
                return Enumerable.Empty<Category>();

            var query = _context.Categories.Where(c => c.ParentCategoryId == parentId);

            if (!query.Any())
            {
                return Enumerable.Empty<Category>();
            }

            if (asNoTracking)
                query = query.AsNoTracking();


            return await query
                .OrderBy(c => c.Name ?? string.Empty)
                .ToListAsync();
        }

        public async Task AddAsync(Category category, bool saveChanges = true)
        {
            await _context.Categories.AddAsync(category);
            if (saveChanges) await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category, bool saveChanges = true)
        {
            _context.Categories.Update(category);
            if (saveChanges) await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category category, bool saveChanges = true)
        {
            _context.Categories.Remove(category);
            if (saveChanges) await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
