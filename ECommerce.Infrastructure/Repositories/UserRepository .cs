using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public class UserRepository:IUserRepository
    {
        private readonly ECommerceDbContext _context;

        public UserRepository(ECommerceDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetByIdAsync(int id, bool asNoTracking = false)
        {
            var query = _context.Users.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<User?> GetByEmailAsync(string email, bool asNoTracking = false)
        {
            var query = _context.Users.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<IEnumerable<User>> GetAllAsync(bool asNoTracking = false)
        {
            var query = _context.Users.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.ToListAsync();
        }
        public async Task<IEnumerable<User>> GetAllAsync(int page = 1, string search = "", int pageSize = 10, bool asNoTracking = false)
        {
            var query = _context.Users.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => EF.Functions.Like(u.UserName, $"%{search}%") ||
                                        EF.Functions.Like(u.Email, $"%{search}%"));

            return await query
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<IEnumerable<User>> GetAllAsync(string search, bool asNoTracking = false)
        {
            var query = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => EF.Functions.Like(u.UserName, $"%{search}%") ||
                                        EF.Functions.Like(u.Email, $"%{search}%"));
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.ToListAsync();
        }

        public async Task AddAsync(User user, bool saveChanges = true)
        {
            await _context.Users.AddAsync(user);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(User user, bool saveChanges = true)
        {
            _context.Users.Update(user);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(User user, bool saveChanges = true)
        {
            _context.Users.Remove(user);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<User?> GetUserWithAddressesAsync(int userId, bool asNoTracking = false)
        {
            var query = _context.Users
                .Include(u => u.UserAddresses)
                .AsQueryable();

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<User?> GetUserWithOrdersAsync(int userId, bool asNoTracking = false)
        {
            var query = _context.Users
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderItems)
                .AsQueryable();

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<User?> GetUserWithWishlistAsync(int userId, bool asNoTracking = false)
        {
            var query = _context.Users
                .Include(u => u.Wishlists)
                    .ThenInclude(w => w.Product)
                .AsQueryable();

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null)
        {
            var query = _context.Users.Where(u => u.Email == email);

            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return !await query.AnyAsync();
        }

        public async Task<User?> GetByResetCodeAsync(string resetCode, bool asNoTracking = false)
        {
            var query = _context.Users.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(u => u.ResetPasswordCode == resetCode &&
                                                       u.ResetPasswordExpiry > DateTime.UtcNow);
        }

    }
}
