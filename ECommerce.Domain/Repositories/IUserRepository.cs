using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, bool asNoTracking = false);
        Task<User?> GetByEmailAsync(string email, bool asNoTracking = false);
        Task<IEnumerable<User>> GetAllAsync(bool asNoTracking = false);
        Task<IEnumerable<User>> GetAllAsync(int page, string search, int pageSize, bool asNoTracking = false);
        Task<IEnumerable<User>> GetAllAsync(string search, bool asNoTracking = false);
        Task AddAsync(User user, bool saveChanges = true);
        Task UpdateAsync(User user, bool saveChanges = true);
        Task DeleteAsync(User user, bool saveChanges = true);
        Task SaveChangesAsync();

        Task<User?> GetUserWithAddressesAsync(int userId, bool asNoTracking = false);
        Task<User?> GetUserWithOrdersAsync(int userId, bool asNoTracking = false);
        Task<User?> GetUserWithWishlistAsync(int userId, bool asNoTracking = false);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);
        Task<User?> GetByResetCodeAsync(string resetCode, bool asNoTracking = false);
    }
}