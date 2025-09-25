using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces.Repositories
{
    public interface IWishlistRepository
    {
        // -------------------- Wishlist --------------------
        Task<Wishlist> CreateWishlistAsync(Wishlist wishlist, bool saveChanges = true);
        Task<bool> DeleteWishlistAsync(int wishlistId, bool saveChanges = true);

        // -------------------- WishlistItem --------------------
        Task AddItemAsync(WishlistItem item, bool saveChanges = true);
        public  Task<bool> DeleteItemAsync(int wishlistId, int productId, bool saveChanges = true);
        Task<IEnumerable<WishlistItem>> GetByUserIdAsync(int userId, bool asNoTracking = true);
        public  Task<int?> GetUserWishlistId(int userId);
        public Task<bool> IsWishlistExist(int WishistId);

        public Task<bool> IsUniqueItem(int wishlistId, int productId);

        // -------------------- Save --------------------
        Task<int> SaveChangesAsync();
    }
}
