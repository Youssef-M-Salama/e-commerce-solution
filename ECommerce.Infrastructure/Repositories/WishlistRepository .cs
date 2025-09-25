using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ECommerceDbContext _context;

        public WishlistRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        // -------------------- Wishlist --------------------
        
        public async Task<Wishlist> CreateWishlistAsync(Wishlist wishlist, bool saveChanges = true)
        {
            await _context.Wishlists.AddAsync(wishlist);

            if (saveChanges)
                await _context.SaveChangesAsync();

            return wishlist;
        }

        public async Task<bool> DeleteWishlistAsync(int wishlistId, bool saveChanges = true)
        {
            var wishlist = await _context.Wishlists.FirstOrDefaultAsync(w => w.Id == wishlistId);
            if (wishlist == null) return false;

            _context.Wishlists.Remove(wishlist);

            if (saveChanges)
                await _context.SaveChangesAsync();

            return true;
        }

        // -------------------- WishlistItem --------------------
        public async Task AddItemAsync(WishlistItem item, bool saveChanges = true)
        {
            await _context.WishlistItems.AddAsync(item);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteItemAsync(int wishlistId, int productId, bool saveChanges = true)
        {
            var item = await _context.WishlistItems.FirstOrDefaultAsync(i => i.WishlistId == wishlistId && i.ProductId == productId);
            if (item == null) return false;

            _context.WishlistItems.Remove(item);

            if (saveChanges)
                await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<WishlistItem>> GetByUserIdAsync(int userId, bool asNoTracking = true)
        {
            var query = _context.Wishlists
                .Include(w => w.Items)
                    .ThenInclude(i => i.Product)
                .Where(w => w.UserId == userId)
                .SelectMany(w => w.Items);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }
        public async Task<int?> GetUserWishlistId(int userId)
        {
            var quere = await _context.Wishlists.FirstOrDefaultAsync(w=>w.User.Id == userId);
            var id = quere?.Id;
            return id;
        }
        public async Task<bool> IsWishlistExist(int WishistId)
        {
            return await _context.Wishlists.AnyAsync(w => w.Id == WishistId);
        }
        public async Task<bool> IsUniqueItem(int wishlistId, int productId)
        {
            return !await _context.WishlistItems.AnyAsync(i=>i.ProductId == productId&&i.WishlistId==wishlistId);
        }



        // -------------------- Save --------------------
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
