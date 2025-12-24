using System.Security.Claims;
using DataLayer.DTOs;
using DataLayer.Intarfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("user/{login}")]
        public async Task<IActionResult> GetOrdersByUser(string login)
        {
            var currentUserLogin = User.FindFirst(ClaimTypes.Name)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserLogin != login &&
                currentUserRole != "Администратор" &&
                currentUserRole != "Менеджер")
            {
                return Forbid("У вас нет прав для просмотра заказов другого пользователя");
            }

            var orders = await _orderService.GetOrdersByUserLoginAsync(login);

            if (orders == null || orders.Count == 0)
                return Ok(new List<OrderDto>());

            return Ok(orders);
        }

        [HttpPut("{orderNumber}")]
        [Authorize(Roles = "Администратор, Менеджер")]
        public async Task<IActionResult> UpdateOrder(string orderNumber, [FromBody] UpdateOrderDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _orderService.UpdateOrderStatusAndDeliveryAsync(
                    orderNumber, updateDto.StatusId, updateDto.DeliveryDate);
                
                return Ok(new { Message = "Заказ успешно обновлен" });
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("не найден"))
                    return NotFound(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}