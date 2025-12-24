using System.ComponentModel.DataAnnotations;

namespace DataLayer.DTOs
{
    public class UpdateOrderDto
    {
        [Required(ErrorMessage = "ID статуса обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "ID статуса должен быть положительным числом")]
        public int StatusId { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "Неверный формат даты")]
        public DateTime? DeliveryDate { get; set; }
    }
}
