using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public static class OrderModel
    {
        public record OrderRequest(Guid CustomerId, string ShippingAddress, string BillingAddress, ICollection<OrderItemRequest> OrderItems);
        public record OrderItemRequest(Guid ProductId, int Quantity, decimal UnitPrice);
        public record Response(Guid Id, Guid CustomerId, string? ShippingAddress, string? BillingAddress, decimal TotalAmount, DateTime? Date, ICollection<OrderItem> OrderItems);
    }
}
