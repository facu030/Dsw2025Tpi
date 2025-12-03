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

        // Request que recibe el endpoint de creación de orden (modo viejo, con CustomerId)
        public record OrderRequest(
            Guid CustomerId,
            string ShippingAddress,
            string BillingAddress,
            ICollection<OrderItemModel.OrderItemRequest> OrderItems
        );

        // ⭐ NUEVO: Request para el endpoint que trabaja por USUARIO (sin CustomerId en el body)
        public record OrderFromUserRequest(
            string ShippingAddress,
            string BillingAddress,
            ICollection<OrderItemModel.OrderItemRequest> OrderItems
        );

        public record AddResponse(
        Guid Id,
        Guid? CustomerId,   // 👈 ahora nullable
        string? ShippingAddress,
        string? BillingAddress,
        decimal TotalAmount,
        DateTime? Date,
        ICollection<OrderItem> OrderItems
    );

        public record GetResponse(
            Guid Id,
            Guid? CustomerId,   // 👈 ahora nullable también
            decimal TotalAmount,
            DateTime? Date,
            ICollection<OrderItemModel.Response> OrderItems
        );


        // ---------------------------
        // 2) Listado para el dashboard (cards)
        // ---------------------------

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

        public record FilterOrder(
            string? Status,
            string? Search,
            int? PageNumber,
            int? PageSize
        );

        public record ResponsePagination(
            List<OrderResponseEasy> OrderItems,
            int Total
        );
    }
}
