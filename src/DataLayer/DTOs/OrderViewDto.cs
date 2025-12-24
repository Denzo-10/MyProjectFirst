using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.DTOs
{
    public class OrderViewDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string OrderStatus { get; set; }
        public string UserFullName { get; set; }
        public List<OrderProductDto> Products { get; set; } = new List<OrderProductDto>();
        public decimal TotalPrice { get; set; }
    }
}
