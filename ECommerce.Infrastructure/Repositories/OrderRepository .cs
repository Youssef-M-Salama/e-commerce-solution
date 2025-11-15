using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ECommerceDbContext _context;

        public OrderRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(int id, bool asNoTracking = false)
        {
            var query = _context.Orders.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber, bool asNoTracking = false)
        {
            var query = _context.Orders.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<IEnumerable<Order>> GetAllAsync(bool asNoTracking = false)
        {
            var query = _context.Orders.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllAsync(int page = 1, string search = "", int pageSize = 10, bool asNoTracking = false)
        {
            var query = _context.Orders.Include(o => o.User).AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(o => EF.Functions.Like(o.OrderNumber, $"%{search}%") ||
                                         EF.Functions.Like(o.User.UserName, $"%{search}%") ||
                                         EF.Functions.Like(o.User.Email, $"%{search}%"));

            return await query.OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllAsync(string search, bool asNoTracking = false)
        {
            var query = _context.Orders.Include(o => o.User).AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(o => EF.Functions.Like(o.OrderNumber, $"%{search}%") ||
                                         EF.Functions.Like(o.User.UserName, $"%{search}%") ||
                                         EF.Functions.Like(o.User.Email, $"%{search}%"));
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task AddAsync(Order order, bool saveChanges = true)
        {
            await _context.Orders.AddAsync(order);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order, bool saveChanges = true)
        {
            _context.Orders.Update(order);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Order order, bool saveChanges = true)
        {
            _context.Orders.Remove(order);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<Order?> GetOrderWithItemsAsync(int orderId, bool asNoTracking = false)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId, bool asNoTracking = false)
        {
            var query = _context.Orders.Where(o => o.UserId == userId).AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId, int page, int pageSize, bool asNoTracking = false)
        {
            var query = _context.Orders.Where(o => o.UserId == userId).AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, bool asNoTracking = false)
        {
            var query = _context.Orders.Where(o => o.Status == status).AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByPaymentStatusAsync(PaymentStatus paymentStatus, bool asNoTracking = false)
        {
            var query = _context.Orders.Where(o => o.PaymentStatus == paymentStatus).AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<bool> IsOrderNumberUniqueAsync(string orderNumber)
        {
            return !await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            string orderNumber;
            do
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var random = new Random().Next(100, 999);
                orderNumber = $"ORD-{timestamp}-{random}";
            }
            while (!await IsOrderNumberUniqueAsync(orderNumber));
            return orderNumber;
        }
    }
}