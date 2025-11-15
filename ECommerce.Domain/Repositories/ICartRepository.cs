using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    public interface ICartRepository
    {
        public Task CreateAsync(Cart cart, bool saveChanges = true)
        ;
        public Task DeleteAsync(Cart cart, bool saveChanges = true);
        public  Task<bool> ClearCartAsync(int userId, bool saveChanges = true);

        // -------------------- CartItem --------------------
        public Task AddItemAsync(CartItem item, bool saveChanges = true);
        public Task<bool> DeleteAsync(int cartId, int productId, bool saveChanges = true);
        public Task<IEnumerable<CartItem>> GetByUserIdAsync(int userId, bool asNoTracking = false);
        public Task<int?> GetUserCartId(int userId);
        public Task updateItemQuantityAsync(int cartId, int proudctId, int itemQuantity, bool saveChanges = true);
        public Task<CartItem?> GetCartItem(int cartId, int productId);

        public Task<bool> IsCartExist(int cartId);

        public Task<bool> IsUniqueItem(int cartId, int productId);

        // -------------------- Save --------------------
        public Task<int> SaveChangesAsync();
    }
}
