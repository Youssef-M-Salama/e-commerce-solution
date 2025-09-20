using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Repositories
{
    public interface IUserAddressRepository
    {
        Task<UserAddress?> GetByIdAsync(int id, bool asNoTracking = false);
        Task<IEnumerable<UserAddress>> GetByUserIdAsync(int userId, bool asNoTracking = false);
        Task<IEnumerable<UserAddress>> GetAllAsync(bool asNoTracking = false);

        Task AddAsync(UserAddress address, bool saveChanges = true);
        Task UpdateAsync(UserAddress address, bool saveChanges = true);
        Task DeleteAsync(UserAddress address, bool saveChanges = true);
        Task SaveChangesAsync();
    }
}
