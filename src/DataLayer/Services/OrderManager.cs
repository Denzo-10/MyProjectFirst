using System.Net.Http;
using System.Security.Claims;
using DataLayer.Context;
using DataLayer.DTOs;
using DataLayer.Intarfaces;
using DataLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Services
{
    public class OrderManager
    {
        private readonly AppDbContext _context;
        private readonly IOrderService _orderService;
        private readonly Random _random;

        public OrderManager(AppDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
            _random = new Random();
        }

        public async Task<OrderDto> CreateOrderAsync(int userId, int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new ArgumentException($"Товар с ID {productId} не найден");

            // Получаем пользователя
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException($"Пользователь с ID {userId} не найден");

            // Получаем статус "Новый"
            var newStatus = await _context.OrderStatuses
                .FirstOrDefaultAsync(s => s.Name == "Новый");

            if (newStatus == null)
            {
                newStatus = new OrderStatuses { Name = "Новый" };
                await _context.OrderStatuses.AddAsync(newStatus);
                await _context.SaveChangesAsync();
            }

            // Генерируем уникальный номер заказа
            var orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{_random.Next(1000, 9999)}";

            // Создаем заказ
            var order = new Order
            {
                OrderNumber = orderNumber,
                OrderDate = DateTime.Now,
                DeliveryDate = DateTime.Now.AddDays(7),
                PickupCode = _random.Next(100, 1000).ToString(),
                OrderStatusesId = newStatus.OrderStatusesId,
                UserId = userId
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Добавляем товар в заказ
            var orderProduct = new OrderProduct
            {
                OrderId = order.OrderId,
                ProductId = productId,
                Quantity = quantity,
                Price = product.Price
            };

            await _context.OrderProducts.AddAsync(orderProduct);
            await _context.SaveChangesAsync();

            // Возвращаем DTO заказа
            return new OrderDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                DeliveryDate = order.DeliveryDate,
                OrderStatus = newStatus.Name,
                UserFullName = user.FullName,
                Products = new List<OrderProductDto>
                {
                    new OrderProductDto
                    {
                        ProductName = product.Name,
                        Quantity = quantity,
                        Price = product.Price
                    }
                }
            };
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(string userLogin)
        {
            return await _orderService.GetOrdersByUserLoginAsync(userLogin);
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            // Поскольку IOrderService не имеет метода GetAllOrders,
            // реализуем его здесь
            var orders = await _context.Orders
                .Include(o => o.OrderStatuses)
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => new OrderDto
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
            }).ToList();
        }

        public string GetCurrentUserLogin(HttpContext httpContext)
        {
            return httpContext.User?.Identity?.Name;
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderNumber, int statusId, DateTime? deliveryDate = null)
        {
            try
            {
                await _orderService.UpdateOrderStatusAndDeliveryAsync(orderNumber, statusId, deliveryDate);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}