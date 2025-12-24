using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using WebApp.Services;
using DataLayer.DTOs;
using System.Security.Claims;

namespace WebApp.Pages.Orders
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly OrderManager _orderManager;

        public List<OrderViewDto> Orders { get; set; } = new List<OrderViewDto>();
        public string UserRole { get; set; }
        public string UserFullName { get; set; }

        public IndexModel(OrderManager orderManager)
        {
            _orderManager = orderManager;
        }

        public async Task OnGetAsync()
        {
            UserRole = HttpContext.Session.GetString("UserRole") ?? "";
            UserFullName = HttpContext.Session.GetString("UserFullName") ?? "";

            var userLogin = User?.Identity?.Name;

            if (!string.IsNullOrEmpty(userLogin))
            {
                List<OrderDto> orderDtos;

                if (UserRole == "Клиент")
                {
                    orderDtos = await _orderManager.GetUserOrdersAsync(userLogin);
                }
                else if (UserRole == "Менеджер" || UserRole == "Администратор")
                {
                    orderDtos = await _orderManager.GetAllOrdersAsync();
                }
                else
                {
                    orderDtos = new List<OrderDto>();
                }

                Orders = orderDtos.Select(o => new OrderViewDto
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    DeliveryDate = o.DeliveryDate,
                    OrderStatus = o.OrderStatus,
                    UserFullName = o.UserFullName,
                    Products = o.Products,
                    TotalPrice = o.Products.Sum(p => p.Price * p.Quantity)
                }).ToList();
            }
        }
    }
}