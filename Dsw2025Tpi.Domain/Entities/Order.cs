using Dsw2025Tpi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Order : EntityBase
    {
        // ✅ NUEVO: constructor vacío para el flujo por USUARIO
        public Order()
        {
            Date = DateTime.UtcNow;
            Status = OrderStatus.Pending;
        }

        // ✅ Constructor viejo: para el flujo con CustomerId (sigue funcionando)
        public Order(Guid customerId) : this()
        {
            CustomerId = customerId;
        }

        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public decimal TotalAmount => OrderItems?.Sum(item => item.Subtotal) ?? 0;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // ✅ AHORA nullable, porque en el flujo por usuario NO siempre hay customer
        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // ✅ NUEVO: para asociar la orden al usuario autenticado (AspNetUser)
        public string? UserId { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
