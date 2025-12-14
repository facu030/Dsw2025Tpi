using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Dsw2025Tpi.Application.Dtos
{
    public static class OrderModel
    {
        // Request que recibe el endpoint de creación de orden (con CustomerId)
        public record OrderRequest(
            Guid CustomerId,
            string ShippingAddress,
            string BillingAddress,
            ICollection<OrderItemModel.OrderItemRequest> OrderItems
        );

        // Request para el endpoint que trabaja con USUARIO (sin CustomerId)
        public record OrderFromUserRequest(
            string ShippingAddress,
            string BillingAddress,
            ICollection<OrderItemModel.OrderItemRequest> OrderItems
        );

        public record AddResponse(
        Guid Id,
        Guid? CustomerId,   //puede ser null
        string? ShippingAddress,
        string? BillingAddress,
        decimal TotalAmount,
        DateTime? Date,
        ICollection<OrderItem> OrderItems
    );

        public record GetResponse(
            Guid Id,
            Guid? CustomerId,   //nulleable
            decimal TotalAmount,
            DateTime? Date,
            ICollection<OrderItemModel.Response> OrderItems
        );


        public record OrderResponseEasy(
            Guid Id,
            string CustomerName,
            DateTime Date,
            string Status,       // "Pending", "Completed", "Canceled"
            decimal TotalAmount
        );


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
