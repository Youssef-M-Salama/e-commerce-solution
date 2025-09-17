//using ECommerce.Domain.Entities;
//using ECommerce.Domain.Repositories;
//using ECommerce.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ECommerce.Infrastructure.Repositories
//{
//    public class SubCategoryRepository : ISubCategoryRepository
//    {
//        private readonly ECommerceDbContext _context;

//        public SubCategoryRepository(ECommerceDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IEnumerable<SubCategory>> GetAllAsync(bool asNoTracking = false)
//        {
//            var query = _context.SubCategories.AsQueryable();
//            if (asNoTracking)
//                query = query.AsNoTracking();

//            return await query.ToListAsync();
//        }

//        public async Task<IEnumerable<SubCategory>> GetAllAsync(int categoryId, bool asNoTracking = false)
//        {
//            var query = _context.SubCategories.Where(sc => sc.CategoryId == categoryId);
//            if (asNoTracking)
//                query = query.AsNoTracking();

//            return await query.ToListAsync();
//        }

//        public async Task<IEnumerable<SubCategory>> GetAllAsync(
//            int page = 1,
//            string search = "",
//            int? categoryId = null,
//            int pageSize = 10,
//            bool asNoTracking = false)
//        {
//            var query = _context.SubCategories.AsQueryable();

//            if (!string.IsNullOrWhiteSpace(search))
//                query = query.Where(sc => EF.Functions.Like(sc.Name, $"%{search}%"));

//            if (categoryId.HasValue)
//                query = query.Where(sc => sc.CategoryId == categoryId.Value);

//            if (asNoTracking)
//                query = query.AsNoTracking();

//            return await query
//                .OrderBy(sc => sc.Name)
//                .Skip((page - 1) * pageSize)
//                .Take(pageSize)
//                .ToListAsync();
//        }

//        public async Task<SubCategory?> GetByIdAsync(int id, bool asNoTracking = false)
//        {
//            var query = _context.SubCategories.AsQueryable();

//            if (asNoTracking)
//                query = query.AsNoTracking();

//            return await query.FirstOrDefaultAsync(sc => sc.Id == id);
//        }

//        public async Task AddAsync(SubCategory subCategory, bool saveChanges = true)
//        {
//            await _context.SubCategories.AddAsync(subCategory);

//            if (saveChanges)
//                await _context.SaveChangesAsync();
//        }

//        public async Task UpdateAsync(SubCategory subCategory, bool saveChanges = true)
//        {
//            _context.SubCategories.Update(subCategory);

//            if (saveChanges)
//                await _context.SaveChangesAsync();
//        }

//        public async Task DeleteAsync(SubCategory subCategory, bool saveChanges = true)
//        {
//            _context.SubCategories.Remove(subCategory);

//            if (saveChanges)
//                await _context.SaveChangesAsync();
//        }

//        public async Task SaveChangesAsync()
//        {
//            await _context.SaveChangesAsync();
//        }
//    }
//}
