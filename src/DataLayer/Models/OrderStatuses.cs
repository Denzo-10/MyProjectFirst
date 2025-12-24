using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class OrderStatuses
{
    public int OrderStatusesId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
