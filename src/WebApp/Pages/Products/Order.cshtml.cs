using DataLayer.Context;
using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace WebApp.Pages.Products
{
    [Authorize(Roles = "Клиент")]
    public class OrderModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly Random _random = new Random();

        [BindProperty(SupportsGet = true)] 
        public int ProductId { get; set; }

        [BindProperty]
        public int Quantity { get; set; } = 1;

        public Product Product { get; set; }
        public string ErrorMessage { get; set; }

        public OrderModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login", new { returnUrl = $"/Products/Order?productId={ProductId}" });
            }

            if (!User.IsInRole("Клиент"))
            {
                TempData["ErrorMessage"] = "Только клиенты могут оформлять заказы";
                return RedirectToPage("/Products/Index");
            }

            if (ProductId <= 0)
            {
                TempData["ErrorMessage"] = "Не указан товар для заказа";
                return RedirectToPage("/Products/Index");
            }

            Product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductId == ProductId);

            if (Product == null)
            {
                TempData["ErrorMessage"] = "Товар не найден";
                return RedirectToPage("/Products/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return await OnGetAsync();
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            if (!User.IsInRole("Клиент"))
            {
                TempData["ErrorMessage"] = "Только клиенты могут оформлять заказы";
                return RedirectToPage("/Products/Index");
            }

            try
            {
                // Проверяем данные формы
                if (Quantity <= 0)
                {
                    ModelState.AddModelError("Quantity", "Количество должно быть больше 0");
                    return await OnGetAsync();
                }

                // Получаем текущего пользователя
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    TempData["ErrorMessage"] = "Ошибка идентификации пользователя";
                    return RedirectToPage("/Login");
                }

                // Получаем товар
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == ProductId);

                if (product == null)
                {
                    ModelState.AddModelError("", "Товар не найден");
                    return await OnGetAsync();
                }

                // Проверяем наличие товара на складе
                if (product.StockQuantity.HasValue && product.StockQuantity.Value < Quantity)
                {
                    ModelState.AddModelError("Quantity",
                        $"Недостаточно товара на складе. Доступно: {product.StockQuantity} шт.");
                    return await OnGetAsync();
                }

                // Получаем статус "Новый"
                var status = await _context.OrderStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Новый");

                if (status == null)
                {
                    status = new OrderStatuses { Name = "Новый" };
                    _context.OrderStatuses.Add(status);
                    await _context.SaveChangesAsync();
                }

                // Генерируем уникальный номер заказа
                string orderNumber;
                bool isUnique;
                do
                {
                    orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{_random.Next(1000, 9999)}";
                    isUnique = !await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber);
                } while (!isUnique);

                // Создаем заказ
                var order = new DataLayer.Models.Order
                {
                    OrderNumber = orderNumber,
                    OrderDate = DateTime.Now,
                    DeliveryDate = DateTime.Now.AddDays(7),
                    PickupCode = _random.Next(100, 1000).ToString(),
                    OrderStatusesId = status.OrderStatusesId,
                    UserId = userId
                };

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                // Добавляем товар в заказ
                var orderProduct = new OrderProduct
                {
                    OrderId = order.OrderId,
                    ProductId = ProductId,
                    Quantity = Quantity,
                    Price = product.Price
                };

                await _context.OrderProducts.AddAsync(orderProduct);

                // Уменьшаем количество товара на складе
                //Не включено, тк не требует задание, но как функционал присутствует
                /*if (product.StockQuantity.HasValue)
                {
                    product.StockQuantity -= Quantity;
                    _context.Products.Update(product);
                }*/

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Заказ №{order.OrderNumber} успешно создан!<br>" +
                                           $"Код получения: <strong>{order.PickupCode}</strong><br>" +
                                           $"Дата доставки: {order.DeliveryDate?.ToString("dd.MM.yyyy")}";
                return RedirectToPage("/Orders/Index");
            }
            catch (DbUpdateException dbEx)
            {
                ModelState.AddModelError("", $"Ошибка базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}");
                return await OnGetAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при создании заказа: {ex.Message}");
                return await OnGetAsync();
            }
        }
    }
}