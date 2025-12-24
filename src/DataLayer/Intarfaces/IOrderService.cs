using DataLayer.DTOs;

namespace DataLayer.Intarfaces
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetOrdersByUserLoginAsync(string login);
        Task UpdateOrderStatusAndDeliveryAsync(string orderNumber, int statusId, DateTime? deliveryDate);
    }
}
