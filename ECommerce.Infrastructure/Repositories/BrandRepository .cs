using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly ECommerceDbContext _context;

        public BrandRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Brand brand, bool saveChanges = true)
        {
            await _context.Brands.AddAsync(brand);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Brand brand, bool saveChanges = true)
        {
            _context.Brands.Remove(brand);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Brand>> GetAllAsync(bool asNoTracking = false)
        {
            var query = _context.Brands.AsQueryable();

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Brand>> GetAllAsync(
            int page = 1,
            string search = "",
            int pageSize = 10,
            bool asNoTracking = false)
        {
            var query = _context.Brands.AsQueryable();

            if (asNoTracking)
                query = query.AsNoTracking();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(b => EF.Functions.Like(b.Name, $"%{search}%"));

            return await query
                .OrderBy(b => b.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<IEnumerable<Brand>> GetAllAsync(string search, bool asNoTracking = false)
        {
            var query=_context.Brands.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => EF.Functions.Like(b.Name, $"%{search}%"));
            }
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.ToListAsync();
        }

        public async Task<Brand?> GetByIdAsync(int id, bool asNoTracking = false)
        {
            var query = _context.Brands.AsQueryable();

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task UpdateAsync(Brand brand, bool saveChanges = true)
        {
            _context.Brands.Update(brand);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
