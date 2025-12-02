using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Enums;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class OrdersManagementService : IOrdersManagementService
    {
        private readonly IRepository _orderRepository;
        private readonly ILogger<OrdersManagementService> _logger;

        public OrdersManagementService(
            IRepository orderRepository,
            ILogger<OrdersManagementService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }
        public async Task<IEnumerable<OrderModel.GetResponse>?> GetOrders()
        {
            var orders = await _orderRepository.GetAll<Order>($"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}");
            return orders?.Select(o => new OrderModel.GetResponse(
                o.Id,
                o.CustomerId,
                //o.ShippingAddress,
                //o.BillingAddress,
                o.TotalAmount,
                o.Date,
                o.OrderItems.Select(item => new OrderItemModel.Response(
                    item.ProductId,
                    item.Product.Name,
                    item.Quantity,
                    item.UnitPrice,
                    item.Subtotal))
                .ToList()
            ));
        }

        public async Task<OrderModel.GetResponse?> GetOrderById(Guid id)
        {
            var order = await _orderRepository.GetById<Order>(id, $"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}");
            return order != null ?
                new OrderModel.GetResponse(
                    order.Id,
                    order.CustomerId,
                    //order.ShippingAddress,
                    //order.BillingAddress,
                    order.TotalAmount,
                    order.Date,
                    order.OrderItems.Select(item => new OrderItemModel.Response(
                        item.ProductId,
                        item.Product.Name,
                        item.Quantity,
                        item.UnitPrice,
                        item.Subtotal
                    )).ToList()
                ) :
                null;
        }

        public async Task<OrderModel.AddResponse?> CreateOrder(OrderModel.OrderRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.ShippingAddress))
                throw new ArgumentException("La dirección de envío no puede estar vacía.");

            if (string.IsNullOrWhiteSpace(request.BillingAddress))
                throw new ArgumentException("La dirección de facturación no puede estar vacía.");

            if (request.OrderItems == null || !request.OrderItems.Any())
                throw new ArgumentException("La orden debe contener al menos un producto.");

            // 1) Validar cliente
            var customer = await _orderRepository.GetById<Customer>(request.CustomerId);
            if (customer == null)
                throw new EntityNotFoundException($"No existe un cliente con el ID {request.CustomerId}");

            // 2) Validar duplicados
            var duplicateProductIds = request.OrderItems
                .GroupBy(x => x.ProductId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateProductIds.Any())
                throw new DuplicatedEntityException("La orden contiene productos duplicados");

            // 3) Traer productos involucrados
            var productIds = request.OrderItems.Select(x => x.ProductId).Distinct().ToList();
            var products = (await _orderRepository.GetFiltered<Product>(p => productIds.Contains(p.Id)))?.ToList()
                           ?? new List<Product>();

            // 4) Validaciones por item
            foreach (var item in request.OrderItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product is null)
                    throw new EntityNotFoundException($"Producto con ID {item.ProductId} no fue encontrado.");

                if (item.Quantity <= 0)
                    throw new ArgumentException($"La cantidad para '{product.Name}' debe ser mayor que 0.");

                if (item.Quantity > product.StockQuantity)
                    throw new ArgumentException(
                        $"No hay stock suficiente para '{product.Name}'. Solicitado: {item.Quantity}, disponible: {product.StockQuantity}"
                    );

                if (item.UnitPrice != product.CurrentUnitPrice)
                    throw new ArgumentException(
                        $"El precio de '{product.Name}' no coincide con el actual. Esperado: {product.CurrentUnitPrice}, recibido: {item.UnitPrice}"
                    );
            }

            // 5) Descontar stock
            foreach (var item in request.OrderItems)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.StockQuantity -= item.Quantity;
                await _orderRepository.Update(product);
            }

            // 6) Crear orden y setear direcciones
            var order = new Order(request.CustomerId);
            order.ShippingAddress = request.ShippingAddress;
            order.BillingAddress = request.BillingAddress;
          

            order.OrderItems = request.OrderItems
                .Select(item => new OrderItem(item.ProductId, item.Quantity, item.UnitPrice))
                .ToList();

            // 7) Guardar
            var createdOrder = await _orderRepository.Add(order);

            // 8) Respuesta
            return new OrderModel.AddResponse(
                createdOrder.Id,
                createdOrder.CustomerId,
                createdOrder.ShippingAddress,
                createdOrder.BillingAddress,
                createdOrder.TotalAmount,
                createdOrder.Date,
                createdOrder.OrderItems
            );
        }
        // === NUEVO: obtener todas las órdenes SOLO para listar (sin detalle) ===
        public async Task<OrderModel.ResponsePagination> GetOrders(OrderModel.FilterOrder request)
        {
            _logger.LogInformation("Iniciando GetOrders (dashboard) con filtros {@request}", request);

            // 1) Traducir Status (string) a enum OrderStatus? (o null si viene "all" o vacío)
            OrderStatus? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                !string.Equals(request.Status, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<OrderStatus>(request.Status, true, out var parsedStatus))
                {
                    statusFilter = parsedStatus;
                }
            }

            // 2) Consulta a BD con GetFiltered, incluyendo solo Customer
            var filteredOrders = await _orderRepository.GetFiltered<Order>(
           o =>
               (statusFilter == null || o.Status == statusFilter) &&
               (string.IsNullOrEmpty(request.Search) ||
                (o.Customer != null && o.Customer.Name.Contains(request.Search))),
           nameof(Order.Customer),
           nameof(Order.OrderItems)
       );


            // 3) Si no hay órdenes, devolvemos estructura vacía
            if (filteredOrders is null || !filteredOrders.Any())
            {
                return new OrderModel.ResponsePagination(
                    new List<OrderModel.OrderResponseEasy>(),
                    0
                );
            }

            // 4) Proyección: mapeamos a OrderResponseEasy
            var pageNumber = request.PageNumber ?? 1;
            var pageSize = request.PageSize ?? 10;

            var ordersMapped = filteredOrders
                .Select(order => new OrderModel.OrderResponseEasy(
                    order.Id,
                    order.Customer?.Name ?? "Cliente no disponible",
                    order.Date,
                    order.Status.ToString(),
                    order.TotalAmount
                ))
                .OrderByDescending(o => o.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 5) Retornamos la paginación: lista + total (sin paginar)
            return new OrderModel.ResponsePagination(
                ordersMapped,
                filteredOrders.Count()
            );
        }

    }
}
