using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Dsw2025Tpi.Application.Dtos
{
    public static class OrderModel
    {
        // ---------------------------
        // 1) Crear orden (POST)
        // ---------------------------

        // Request que recibe el endpoint de creación de orden
        public record OrderRequest(
            Guid CustomerId,
            string ShippingAddress,
            string BillingAddress,
            ICollection<OrderItemModel.OrderItemRequest> OrderItems
        );

        // Respuesta cuando se crea una orden (puede usarse también internamente)
        public record AddResponse(
            Guid Id,
            Guid CustomerId,
            string? ShippingAddress,
            string? BillingAddress,
            decimal TotalAmount,
            DateTime? Date,
            ICollection<OrderItem> OrderItems
        );

        // Respuesta "completa" para GET /api/orders/{id} o similar
        public record GetResponse(
            Guid Id,
            Guid CustomerId,
            /*string? ShippingAddress,
            string? BillingAddress,*/
            decimal TotalAmount,
            DateTime? Date,
            ICollection<OrderItemModel.Response> OrderItems
        );

        // ---------------------------
        // 2) Listado para el dashboard (cards)
        // ---------------------------

        // Lo que ve el dashboard en cada card del listado de órdenes
        public record OrderResponseEasy(
            Guid Id,
            string CustomerName,
            DateTime Date,
            string Status,       // "Pending" | "Completed" | "Canceled"
            decimal TotalAmount
        );

        // ---------------------------
        // 3) Filtros y paginación
        // ---------------------------

        // Filtros que vienen por query string:
        // ?Status=...&Search=...&PageNumber=1&PageSize=10
        public record FilterOrder(
            string? Status,     // Pending / Completed / Canceled
            string? Search,     // texto para cliente o Id
            int? PageNumber,    // si viene null, en el service se pone 1
            int? PageSize       // si viene null, en el service se pone 10
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
