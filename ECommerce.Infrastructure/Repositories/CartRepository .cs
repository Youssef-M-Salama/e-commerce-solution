using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public class CartRepository: ICartRepository
    {
        private readonly ECommerceDbContext _context;

        public CartRepository(ECommerceDbContext context)
        {
            _context = context;
        }
        // -------------------- Cart --------------------
        public async Task CreateAsync(Cart cart,bool saveChanges=true)
        {
            await _context.Carts.AddAsync(cart);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Cart cart, bool saveChanges = true)
        {
             _context.Carts.Remove(cart);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

        }

        // -------------------- CartItem --------------------
        public async Task AddItemAsync(CartItem item,bool saveChanges = true)
        {
            await _context.CartItems.AddAsync(item);

            if(saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> DeleteAsync(int cartId, int productId, bool saveChanges = true)
        {
            var item = await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
            if (item == null)
            {
               return false;
            }
            _context.Remove(item);
            if(saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return true;
        }
        public async Task<IEnumerable<CartItem>> GetByUserIdAsync(int userId, bool asNoTracking = false)
        {
            var query = _context.Carts
               .Include(w => w.CartItems)
                   .ThenInclude(i => i.Product)
               .Where(w => w.UserId == userId)
               .SelectMany(w => w.CartItems);
            if (asNoTracking)
            {
                return query.AsNoTracking();
            }
            return await query.ToListAsync();
        }
        public async Task<int?> GetUserCartId(int userId)
        {
            var quere = await _context.Carts.FirstOrDefaultAsync(w => w.User.Id == userId);
            var id = quere?.Id;
            return id;
        }
        public async Task updateItemQuantityAsync(int cartId, int proudctId,int itemQuantity,bool saveChanges=true)
        {
            var item = await GetCartItem(cartId,proudctId);
            if (item == null) {
                return; 
            }
            item.Quantity = itemQuantity;
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }
        public async Task<CartItem?>GetCartItem(int cartId,int productId)
        {
            return await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
        }
        public async Task<bool> IsCartExist(int cartId)
        {
            return await _context.Carts.AnyAsync(w => w.Id == cartId);
        }
        public async Task<bool> IsUniqueItem(int cartId, int productId)
        {
            return !await _context.CartItems.AnyAsync(i => i.ProductId == productId && i.CartId == cartId);
        }

        // -------------------- Save --------------------
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
