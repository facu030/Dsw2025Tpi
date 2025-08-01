using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using static Dsw2025Tpi.Application.Dtos.OrderModel;

namespace Dsw2025Tpi.Application.Services
{
    public class OrderManagementService
    {
        private readonly IRepository _repository;

        public OrderManagementService(IRepository repository)
        {
            _repository = repository;
        }

        /*Para agregar un nueva order*/
        public async Task<OrderModel.OrderResponse> AddOrder(OrderModel.OrderRequest request)
        {
            var customer = await _repository.GetById<Customer>(request.CustomerId);
            if (customer == null)
                throw new EntityNotFoundException($"El ID {request.CustomerId} no pertenece a ningun Customer.");

            if (string.IsNullOrWhiteSpace(request.ShippingAddress) ||
                string.IsNullOrWhiteSpace(request.BillingAddress))
            {
                throw new ArgumentException("valor invalido ingresado para Address");
            }

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;
            foreach (var item in request.OrderItems)
            {
                var product = await _repository.GetById<Product>(item.ProductId);
                if (product == null || product.IsActive == false)
                    throw new EntityNotFoundException($"Produc con ID {item.ProductId} no encontrado.");

                if (!product.StockSufficient(item.Quantity))
                    throw new InvalidOperationException($"Stock insuficiente del producto: {product.Name}.");
                product.DecreaseStock(item.Quantity);

                await _repository.Update(product); //actualizo el producto con el stock actual

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = item.Quantity,
                    UnitPrice = product.CurrentUnitPrice
                };
                orderItem.SubTotal = orderItem.CalculateSubTotal();
                totalAmount += orderItem.SubTotal;
                orderItems.Add(orderItem);
            }

            var order = new Order(request.ShippingAddress, request.BillingAddress, request.Notes,orderItems, totalAmount, request.CustomerId);
            await _repository.Add(order);

            var response = new OrderModel.OrderResponse(
                order.Id,
                order.CustomerId,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                orderItems.Select(oi => new OrderModel.OrderItemResponse(
                    oi.ProductId,
                    oi.Product.Name,
                    oi.UnitPrice,
                    oi.Quantity,
                    oi.SubTotal
                )).ToList(),
                order.Status.ToString()
            );
             
            return response;
        }

        /*PARA OBTENER UNA ORDEN POR ID*/
        public async Task<OrderModel.OrderDetailedResponse?> DetailedOrderById(Guid id)
        {
            // Cargamos orden con items, productos y cliente
            var orden = await _repository.GetById<Order>(id, "OrderItems.Product", "Customer");

            if (orden == null) throw new EntityNotFoundException($"Orden con ID {id} no encontrada.");

            return new OrderModel.OrderDetailedResponse(
                Id: orden.Id,
                CustomerId: orden.CustomerId,
                CustomerName: orden.Customer?.Name ?? "Cliente no disponible",
                Date: orden.Date,
                ShippingAddress: orden.ShippingAddress,
                BillingAddress: orden.BillingAddress,
                Notes: orden.Notes ?? string.Empty,
                OrderItems: orden.OrderItems?.Select(oi => new OrderModel.OrderItemResponse(
                    oi.ProductId,
                    oi.Product?.Name ?? "Producto no disponible",
                    oi.UnitPrice, // Corregido de UnifPrice a UnitPrice
                    oi.Quantity,
                    oi.SubTotal
                ))?.ToList() ?? new List<OrderModel.OrderItemResponse>(),
                OrderStatus: orden.Status.ToString(),
                TotalAmount: orden.TotalAmount
            ); // Paréntesis correctamente cerrado
        }

        /*PARA ACTUALIZAR EL ESTADO DE UNA ORDEN*/
        public async Task<OrderModel.OrderDetailedResponse?> UpdateOrderStatus(Guid orderId, OrderStatus newStatus)
        {
            // Obtener la orden con sus relaciones
            var order = await _repository.GetById<Order>(orderId,"OrderItems.Product","Customer");
            if (order == null)
            {
                throw new EntityNotFoundException($"Orden con ID {orderId} no encontrada");
            }

            // Validar transición de estado
            if (!IsValidStatusTransition(order.Status, newStatus))
            {
                throw new InvalidOperationException(
                    $"Transición no permitida de {order.Status} a {newStatus}");
            }

            // actualizo el estado
            order.Status = newStatus;
            await _repository.Update(order);

            
            return new OrderModel.OrderDetailedResponse(
                order.Id,
                order.CustomerId,
                order.Customer?.Name ?? "Cliente no disponible",
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes ?? string.Empty,
                order.OrderItems?.Select(oi => new OrderModel.OrderItemResponse(
                    oi.ProductId,
                    oi.Product?.Name ?? "Producto no disponible",
                    oi.UnitPrice,
                    oi.Quantity,
                    oi.SubTotal
                ))?.ToList() ?? new List<OrderModel.OrderItemResponse>(),
                order.Status.ToString(),
                order.TotalAmount
            );
        }

        /*IMPLEMENTACION DE LAS REGLAS DE TRANSICIÓN*/
       private bool IsValidStatusTransition(OrderStatus current, OrderStatus newStatus)
        {
            // Implementación de reglas de transición
            switch (current)
            {
                case OrderStatus.PENDING:
                    return newStatus == OrderStatus.PROCESSING ||
                           newStatus == OrderStatus.CANCELLED;

                case OrderStatus.PROCESSING:
                    return newStatus == OrderStatus.SHIPPED ||
                           newStatus == OrderStatus.CANCELLED;

                case OrderStatus.SHIPPED:
                    return newStatus == OrderStatus.DELIVERED;

                case OrderStatus.DELIVERED:
                case OrderStatus.CANCELLED:
                    return false; // no permito mas cambios después de enviado y cancelado

                default:
                    throw new ArgumentOutOfRangeException(nameof(current));
            }
        }


        /*PARA OBTENER TODAS LAS ORDENES*/
        public async Task<List<OrderModel.OrderDetailedResponse>> GetAllOrders()
        {
            var orders = await _repository.GetAll<Order>("Customer", "OrderItems.Product");

            return orders.Select(order => new OrderModel.OrderDetailedResponse(
                order.Id,
                order.CustomerId,
                order.Customer?.Name ?? "Cliente no disponible",
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes ?? string.Empty,
                order.OrderItems?.Select(oi => new OrderModel.OrderItemResponse(
                    oi.ProductId,
                    oi.Product?.Name ?? "Producto no disponible",
                    oi.UnitPrice,
                    oi.Quantity,
                    oi.SubTotal
                ))?.ToList() ?? new List<OrderModel.OrderItemResponse>(),
                order.Status.ToString(),
                order.TotalAmount
            )).ToList();
        }

    }
}
