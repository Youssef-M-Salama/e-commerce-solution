using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id, bool asNoTracking = false);
        Task<Order?> GetByOrderNumberAsync(string orderNumber, bool asNoTracking = false);
        Task<IEnumerable<Order>> GetAllAsync(bool asNoTracking = false);
        Task<IEnumerable<Order>> GetAllAsync(int page, string search, int pageSize, bool asNoTracking = false);
        Task<IEnumerable<Order>> GetAllAsync(string search, bool asNoTracking = false);
        Task AddAsync(Order order, bool saveChanges = true);
        Task UpdateAsync(Order order, bool saveChanges = true);
        Task DeleteAsync(Order order, bool saveChanges = true);
        Task SaveChangesAsync();
        Task<Order?> GetOrderWithItemsAsync(int orderId, bool asNoTracking = false);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId, bool asNoTracking = false);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId, int page, int pageSize, bool asNoTracking = false);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, bool asNoTracking = false);
        Task<IEnumerable<Order>> GetOrdersByPaymentStatusAsync(PaymentStatus paymentStatus, bool asNoTracking = false);
        Task<bool> IsOrderNumberUniqueAsync(string orderNumber);
        Task<string> GenerateOrderNumberAsync();
    }
}