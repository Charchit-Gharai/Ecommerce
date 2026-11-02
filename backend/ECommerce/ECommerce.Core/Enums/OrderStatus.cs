using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Core.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Processing = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
        PaymentFailed = 6
    }
}
