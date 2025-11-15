using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly ECommerceDbContext _context;

        public OrderItemRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<OrderItem?> GetByIdAsync(int id, bool asNoTracking = false)
        {
            var query = _context.OrderItems.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(oi => oi.Id == id);
        }

        public async Task<IEnumerable<OrderItem>> GetAllAsync(bool asNoTracking = false)
        {
            var query = _context.OrderItems.AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId, bool asNoTracking = false)
        {
            var query = _context.OrderItems.Include(oi => oi.Product)
                .Where(oi => oi.OrderId == orderId).AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.ToListAsync();
        }

        public async Task AddAsync(OrderItem orderItem, bool saveChanges = true)
        {
            await _context.OrderItems.AddAsync(orderItem);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderItem orderItem, bool saveChanges = true)
        {
            _context.OrderItems.Update(orderItem);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(OrderItem orderItem, bool saveChanges = true)
        {
            _context.OrderItems.Remove(orderItem);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}