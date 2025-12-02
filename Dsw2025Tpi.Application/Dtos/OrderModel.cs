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
        public record OrderRequest(Guid CustomerId, string ShippingAddress, string BillingAddress, ICollection<OrderItemModel.OrderItemRequest> OrderItems);
        public record AddResponse(Guid Id, Guid CustomerId, string? ShippingAddress, string? BillingAddress, decimal TotalAmount, DateTime? Date, ICollection<OrderItem> OrderItems);
        public record GetResponse(Guid Id, Guid CustomerId, /*string? ShippingAddress, string? BillingAddress,*/ decimal TotalAmount, DateTime? Date, ICollection<OrderItemModel.Response> OrderItems);

        // Lo que ve el dashboard en cada card
        public record OrderResponseEasy(
            Guid Id,
            string CustomerName,
            DateTime Date,
            string Status,
            decimal TotalAmount
        );

        // Filtros que vienen por query string:
        // ?Status=...&Search=...&PageNumber=1&PageSize=10
        public record FilterOrder(
            string? Status,
            string? Search,
            int? PageNumber,
            int? PageSize
        );

        // Respuesta paginada:
        // - OrderItems: lista de las órdenes "livianas"
        // - Total: cuántas hay en total según el filtro
        public record ResponsePagination(
            List<OrderResponseEasy> OrderItems,
            int Total
        );
    }
}

