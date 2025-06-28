using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Application.Services
{
    public class OrdersManagementService : IOrderService
    {
        private readonly IRepository _repository;

        public OrdersManagementService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<OrderModel.OrderResponse> AddOrder(OrderModel.OrderRequest request)
        {
            if (request == null ||
                request.CustomerId == Guid.Empty ||
                string.IsNullOrWhiteSpace(request.ShippingAddress) ||
                string.IsNullOrWhiteSpace(request.BillingAddress) ||
                request.OrderItems == null ||
                !request.OrderItems.Any())
            {
                throw new ArgumentException("Los datos de la orden no son válidos.");
            }

            var customer = await _repository.GetById<Customer>(request.CustomerId);
            if (customer == null)
            {
                throw new EntityNotFoundException($"Cliente con ID {request.CustomerId} no encontrado.");
            }

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var itemRequest in request.OrderItems)
            {
                var product = await _repository.GetById<Product>(itemRequest.ProductId);

                if (product == null || !product.IsActive)
                {
                    throw new EntityNotFoundException($"Producto con ID {itemRequest.ProductId} no encontrado o inactivo.");
                }

                if (product.StockQuantity < itemRequest.Quantity)
                {
                    throw new ArgumentException($"Stock insuficiente para el producto {product.Name}. Cantidad disponible: {product.StockQuantity}");
                }

                var subtotal = itemRequest.Quantity * product.CurrentUnitPrice;
                totalAmount += subtotal;

                var orderItem = new OrderItem
                {
                    Quantity = itemRequest.Quantity,
                    UnitPrice = product.CurrentUnitPrice,
                    Subtotal = subtotal,
                    ProductId = product.Id,

                };
                orderItems.Add(orderItem);

                product.StockQuantity -= itemRequest.Quantity;
                await _repository.Update(product);
            }

            var order = new Order(
                shippingAddress: request.ShippingAddress,
                billingAddress: request.BillingAddress,
                notes: request.Notes,
                totalAmount: totalAmount,
                orderItems: orderItems,
                customerId: request.CustomerId
            );

            await _repository.Add(order);

            var responseItems = order.OrderItems.Select(oi => new OrderModel.OrderItemResponse(
                oi.ProductId,
                oi.UnitPrice,
                oi.Quantity,
                oi.Subtotal
            )).ToList();

            return new OrderModel.OrderResponse(
                order.Id,
                order.CustomerId,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.Date,
                order.TotalAmount,
                responseItems,
                order.Status.ToString()
            );
        }
    }
}
