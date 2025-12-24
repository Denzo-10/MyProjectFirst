using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Context;
using DataLayer.DTOs;
using DataLayer.Intarfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDto>> GetOrdersByUserLoginAsync(string login)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.Login == login);

            if (!userExists)
                return new List<OrderDto>();

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderStatuses)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .Where(o => o.User.Login == login)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    DeliveryDate = o.DeliveryDate,
                    OrderStatus = o.OrderStatuses.Name,
                    UserFullName = o.User.FullName,
                    Products = o.OrderProducts.Select(op => new OrderProductDto
                    {
                        ProductName = op.Product.Name,
                        Quantity = op.Quantity,
                        Price = op.Price
                    }).ToList()
                })
                .ToListAsync();

            return orders;
        }

        public async Task UpdateOrderStatusAndDeliveryAsync(
            string orderNumber, int statusId, DateTime? deliveryDate)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null)
                throw new ArgumentException($"Заказ с номером {orderNumber} не найден");

            var statusExists = await _context.OrderStatuses
                .AnyAsync(s => s.OrderStatusesId == statusId); 

            if (!statusExists)
                throw new ArgumentException($"Статус с ID {statusId} не найден");

            order.OrderStatusesId = statusId;

            if (deliveryDate.HasValue)
            {
                if (deliveryDate.Value < order.OrderDate)
                    throw new ArgumentException(
                        "Дата доставки не может быть раньше даты заказа");

                order.DeliveryDate = deliveryDate.Value;
            }

            await _context.SaveChangesAsync();
        }
    }
}