using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int OrderStatusesId { get; set; }

    public int UserId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string? PickupCode { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual OrderStatuses OrderStatuses { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
