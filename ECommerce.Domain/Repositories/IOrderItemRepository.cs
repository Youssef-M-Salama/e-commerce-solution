using ECommerce.Domain.Entities;
namespace ECommerce.Domain.Repositories
{
    public interface IOrderItemRepository
    {
        Task<OrderItem?> GetByIdAsync(int id, bool asNoTracking = false);
        Task<IEnumerable<OrderItem>> GetAllAsync(bool asNoTracking = false);
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId, bool asNoTracking = false);
        Task AddAsync(OrderItem orderItem, bool saveChanges = true);
        Task UpdateAsync(OrderItem orderItem, bool saveChanges = true);
        Task DeleteAsync(OrderItem orderItem, bool saveChanges = true);
        Task SaveChangesAsync();
    }
}