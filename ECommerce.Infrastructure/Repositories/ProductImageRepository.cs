    using ECommerce.Domain.Entities;
    using ECommerce.Domain.Repositories;
    using ECommerce.Infrastructure.Data;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    namespace ECommerce.Infrastructure.Repositories
    {
        /// <summary>
        /// Concrete implementation of <see cref="IProductImageRepository"/>
        /// using Entity Framework Core.
        /// </summary>
        public class ProductImageRepository : IProductImageRepository
        {
            private readonly ECommerceDbContext _context;

            public ProductImageRepository(ECommerceDbContext context)
            {
                _context = context;
            }

            public async Task AddAsync(ProductImage image, bool saveChanges = true)
            {
                await _context.ProductImages.AddAsync(image);
                if (saveChanges) await SaveChangesAsync();
            }

            public async Task DeleteAsync(ProductImage image, bool saveChanges = true)
            {
                _context.ProductImages.Remove(image);
                if (saveChanges) await SaveChangesAsync();
            }

            public async Task UpdateAsync(ProductImage image, bool saveChanges = true)
            {
                _context.ProductImages.Update(image);
                if (saveChanges) await SaveChangesAsync();
            }

            public async Task<IEnumerable<ProductImage>> GetAllByProductIdAsync(int productId, bool asNoTracking = false)
            {
                var query = _context.ProductImages
                                    .Where(pi => pi.ProductId == productId)
                                    .OrderByDescending(pi => pi.IsMain)
                                    .ThenBy(pi => pi.DisplayOrder).AsQueryable();

                if (asNoTracking) query = query.AsNoTracking();

                return await query.ToListAsync();
            }

            public async Task<IEnumerable<ProductImage>> GetAllByProductIdAsync(int productId, int page = 1, int pageSize = 10, bool asNoTracking = false)
            {
                var query = _context.ProductImages
                                    .Where(pi => pi.ProductId == productId)
                                    .OrderByDescending(pi => pi.IsMain)
                                    .ThenBy(pi => pi.DisplayOrder)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize);

                if (asNoTracking) query = query.AsNoTracking();

                return await query.ToListAsync();
            }

            public async Task<ProductImage?> GetMainOrFirstAsync(int productId, bool asNoTracking = false)
            {
                var query = _context.ProductImages
                                    .Where(pi => pi.ProductId == productId);

                if (asNoTracking) query = query.AsNoTracking();

                var mainImage = await query.FirstOrDefaultAsync(pi => pi.IsMain);

                return mainImage ?? await query.OrderBy(pi => pi.DisplayOrder).FirstOrDefaultAsync();
            }

            public async Task<ProductImage?> GetByIdAsync(int id, bool asNoTracking = false)
            {
                var query = _context.ProductImages.Where(pi => pi.Id == id);

                if (asNoTracking) query = query.AsNoTracking();

                return await query.FirstOrDefaultAsync();
            }

            public async Task SetMainAsync(int productId, int imageId, bool saveChanges = true)
            {
                var images = _context.ProductImages.Where(pi => pi.ProductId == productId);

                await images.ForEachAsync(pi => pi.IsMain = false);

                var image = await _context.ProductImages.FirstOrDefaultAsync(pi => pi.Id == imageId && pi.ProductId == productId);
                if (image != null) image.IsMain = true;

                if (saveChanges) await SaveChangesAsync();
            }

            public async Task<bool> DoesDisplayOrderExistAsync(int productId, int displayOrder, int? excludeId = null)
            {
                return await _context.ProductImages
                    .AnyAsync(pi => pi.ProductId == productId
                                 && pi.DisplayOrder == displayOrder
                                 && (excludeId == null || pi.Id != excludeId));
            }

            public async Task<bool> DoesMainImageExistAsync(int productId, int? excludeId = null)
            {
                return await _context.ProductImages
                    .AnyAsync(pi => pi.ProductId == productId
                                 && pi.IsMain
                                 && (excludeId == null || pi.Id != excludeId));
            }

            public async Task SaveChangesAsync()
            {
                await _context.SaveChangesAsync();
            }
        }
    }
