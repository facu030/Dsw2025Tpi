using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Enums;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class OrdersManagementService : IOrdersManagementService
    {
        private readonly IRepository _orderRepository;
        private readonly ILogger<OrdersManagementService> _logger;
        private readonly AuthenticateContext _authContext;

        public OrdersManagementService(
            IRepository orderRepository,
            ILogger<OrdersManagementService> logger,
            AuthenticateContext authContext)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _authContext = authContext;
        }

        // ==== GET ALL (detalle completo) ====
        public async Task<IEnumerable<OrderModel.GetResponse>?> GetOrders()
        {
            var orders = await _orderRepository.GetAll<Order>(
                $"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}"
            );

            return orders?.Select(o => new OrderModel.GetResponse(
                o.Id,
                o.CustomerId,
                o.TotalAmount,
                o.Date,
                o.OrderItems.Select(item => new OrderItemModel.Response(
                    item.ProductId,
                    item.Product.Name,
                    item.Quantity,
                    item.UnitPrice,
                    item.Subtotal
                )).ToList()
            ));
        }

        // ==== GET BY ID (detalle completo) ====
        public async Task<OrderModel.GetResponse?> GetOrderById(Guid id)
        {
            var order = await _orderRepository.GetById<Order>(
                id,
                $"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}"
            );

            return order != null
                ? new OrderModel.GetResponse(
                    order.Id,
                    order.CustomerId,
                    order.TotalAmount,
                    order.Date,
                    order.OrderItems.Select(item => new OrderItemModel.Response(
                        item.ProductId,
                        item.Product.Name,
                        item.Quantity,
                        item.UnitPrice,
                        item.Subtotal
                    )).ToList()
                )
                : null;
        }

        // ==== CREATE (flujo viejo por CustomerId) ====
        public async Task<OrderModel.AddResponse> CreateOrder(OrderModel.OrderRequest request)
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

            // 3) Traer productos
            var productIds = request.OrderItems.Select(x => x.ProductId).Distinct().ToList();
            var products = (await _orderRepository.GetFiltered<Product>(p => productIds.Contains(p.Id)))
                           ?.ToList()
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
                        $"No hay stock suficiente para '{product.Name}'. " +
                        $"Solicitado: {item.Quantity}, disponible: {product.StockQuantity}"
                    );

                if (item.UnitPrice != product.CurrentUnitPrice)
                    throw new ArgumentException(
                        $"El precio de '{product.Name}' no coincide con el actual. " +
                        $"Esperado: {product.CurrentUnitPrice}, recibido: {item.UnitPrice}"
                    );
            }

            // 5) Descontar stock
            foreach (var item in request.OrderItems)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.StockQuantity -= item.Quantity;
                await _orderRepository.Update(product);
            }

            // 6) Crear orden (Customer)
            var order = new Order(request.CustomerId)
            {
                ShippingAddress = request.ShippingAddress,
                BillingAddress = request.BillingAddress
            };

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

        // ==== CREATE (flujo nuevo por usuario autenticado) ====
        public async Task<OrderModel.AddResponse> CreateOrderForUserAsync(
            string userId,
            OrderModel.OrderFromUserRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.ShippingAddress))
                throw new ArgumentException("La dirección de envío no puede estar vacía.");

            if (string.IsNullOrWhiteSpace(request.BillingAddress))
                throw new ArgumentException("La dirección de facturación no puede estar vacía.");

            if (request.OrderItems == null || !request.OrderItems.Any())
                throw new ArgumentException("La orden debe contener al menos un producto.");

            // 1) Validar duplicados
            var duplicateProductIds = request.OrderItems
                .GroupBy(x => x.ProductId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateProductIds.Any())
                throw new DuplicatedEntityException("La orden contiene productos duplicados");

            // 2) Traer productos
            var productIds = request.OrderItems.Select(x => x.ProductId).Distinct().ToList();
            var products = (await _orderRepository.GetFiltered<Product>(p => productIds.Contains(p.Id)))
                           ?.ToList()
                           ?? new List<Product>();

            // 3) Validaciones por item
            foreach (var item in request.OrderItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product is null)
                    throw new EntityNotFoundException($"Producto con ID {item.ProductId} no fue encontrado.");

                if (item.Quantity <= 0)
                    throw new ArgumentException($"La cantidad para '{product.Name}' debe ser mayor que 0.");

                if (item.Quantity > product.StockQuantity)
                    throw new ArgumentException(
                        $"No hay stock suficiente para '{product.Name}'. " +
                        $"Solicitado: {item.Quantity}, disponible: {product.StockQuantity}"
                    );

                if (item.UnitPrice != product.CurrentUnitPrice)
                    throw new ArgumentException(
                        $"El precio de '{product.Name}' no coincide con el actual. " +
                        $"Esperado: {product.CurrentUnitPrice}, recibido: {item.UnitPrice}"
                    );
            }

            // 4) Descontar stock
            foreach (var item in request.OrderItems)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.StockQuantity -= item.Quantity;
                await _orderRepository.Update(product);
            }

            // 5) Crear orden para usuario
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = request.ShippingAddress,
                BillingAddress = request.BillingAddress,
                Status = OrderStatus.Pending,
                Date = DateTime.UtcNow
            };

            order.OrderItems = request.OrderItems
                .Select(item => new OrderItem(item.ProductId, item.Quantity, item.UnitPrice))
                .ToList();

            var createdOrder = await _orderRepository.Add(order);

            return new OrderModel.AddResponse(
                createdOrder.Id,
                createdOrder.CustomerId,        // null en este flujo
                createdOrder.ShippingAddress,
                createdOrder.BillingAddress,
                createdOrder.TotalAmount,
                createdOrder.Date,
                createdOrder.OrderItems
            );
        }

        // ==== LISTADO PAGINADO PARA DASHBOARD ====
        public async Task<OrderModel.ResponsePagination> GetOrders(OrderModel.FilterOrder request)
        {
            _logger.LogInformation("Iniciando GetOrders (dashboard) con filtros {@request}", request);

            // 1) Status → enum nullable
            OrderStatus? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                !string.Equals(request.Status, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<OrderStatus>(request.Status, true, out var parsedStatus))
                {
                    statusFilter = parsedStatus;
                }
            }

            // 2) Traer órdenes filtradas por estado
            var filteredOrders = await _orderRepository.GetFiltered<Order>(
                o => statusFilter == null || o.Status == statusFilter,
                nameof(Order.Customer),
                nameof(Order.OrderItems)
            );

            if (filteredOrders is null || !filteredOrders.Any())
            {
                return new OrderModel.ResponsePagination(
                    new List<OrderModel.OrderResponseEasy>(),
                    0
                );
            }

            var ordersList = filteredOrders.ToList();

            // 3) Buscar usuarios asociados (UserId) y matchear por Id, Email o UserName
            var userIds = ordersList
                .Where(o => o.UserId != null)
                .Select(o => o.UserId!)
                .Distinct()
                .ToList();

            var usersDict = new Dictionary<string, string>();

            if (userIds.Any())
            {
                var users = await _authContext.Users
                    .Where(u =>
                        userIds.Contains(u.Id) ||
                        (u.Email != null && userIds.Contains(u.Email)) ||
                        (u.UserName != null && userIds.Contains(u.UserName))
                    )
                    .ToListAsync();

                foreach (var u in users)
                {
                    // 👇 Prioridad: UserName (nombre) y si no, Email
                    var display = u.UserName ?? u.Email ?? "Usuario sin datos";

                    // mapeamos por Id
                    if (!usersDict.ContainsKey(u.Id))
                        usersDict[u.Id] = display;

                    // por Email
                    if (!string.IsNullOrEmpty(u.Email) && !usersDict.ContainsKey(u.Email))
                        usersDict[u.Email] = display;

                    // por UserName
                    if (!string.IsNullOrEmpty(u.UserName) && !usersDict.ContainsKey(u.UserName))
                        usersDict[u.UserName] = display;
                }
            }

            // 4) Construir nombre a mostrar
            var withDisplayName = ordersList
                .Select(order =>
                {
                    string displayName;

                    if (order.Customer != null && !string.IsNullOrEmpty(order.Customer.Name))
                    {
                        // Flujo viejo (Customer)
                        displayName = order.Customer.Name;
                    }
                    else if (order.UserId != null && usersDict.TryGetValue(order.UserId, out var userName))
                    {
                        // Flujo nuevo (UserId → Identity)
                        displayName = userName;
                    }
                    else
                    {
                        displayName = "Cliente no disponible";
                    }

                    return new { Order = order, DisplayName = displayName };
                })
                .ToList();

            // 5) Aplicar búsqueda (Search) por nombre o Id
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                withDisplayName = withDisplayName
                    .Where(x =>
                        x.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        x.Order.Id.ToString().Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!withDisplayName.Any())
            {
                return new OrderModel.ResponsePagination(
                    new List<OrderModel.OrderResponseEasy>(),
                    0
                );
            }

            var pageNumber = request.PageNumber ?? 1;
            var pageSize = request.PageSize ?? 10;

            // 6) Mapear al DTO y paginar
            var ordersMapped = withDisplayName
                .Select(x => new OrderModel.OrderResponseEasy(
                    x.Order.Id,
                    x.DisplayName,
                    x.Order.Date,
                    x.Order.Status.ToString(),
                    x.Order.TotalAmount
                ))
                .OrderByDescending(o => o.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new OrderModel.ResponsePagination(
                ordersMapped,
                withDisplayName.Count
            );
        }
    }
}
