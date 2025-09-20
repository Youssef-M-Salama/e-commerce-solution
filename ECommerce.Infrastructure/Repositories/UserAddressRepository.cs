using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class UserAddressRepository : IUserAddressRepository
    {
        private readonly ECommerceDbContext _context;

        public UserAddressRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<UserAddress?> GetByIdAsync(int id, bool asNoTracking = false)
        {
            var query = _context.UserAddresses.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<UserAddress>> GetByUserIdAsync(int userId, bool asNoTracking = false)
        {
            var query = _context.UserAddresses.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<UserAddress>> GetAllAsync(bool asNoTracking = false)
        {
            var query = _context.UserAddresses.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task AddAsync(UserAddress address, bool saveChanges = true)
        {
            await _context.UserAddresses.AddAsync(address);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserAddress address, bool saveChanges = true)
        {
            _context.UserAddresses.Update(address);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(UserAddress address, bool saveChanges = true)
        {
            _context.UserAddresses.Remove(address);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
    