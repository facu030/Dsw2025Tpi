using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Enums;
using Dsw2025Tpi.Application.Exceptions;
using static Dsw2025Tpi.Application.Dtos.OrderItemModel;



namespace Dsw2025Tpi.Application.Services;

public class OrderManagementService
{
    private readonly IRepository _repository;

    public OrderManagementService(IRepository repository)
    {
        _repository = repository;
    }

    //Crear una nueva Orden

    public async Task<OrderResponse> CreateOrder(OrderRequest request)
    {

        if (string.IsNullOrWhiteSpace(request.ShippingAddress) ||
            string.IsNullOrWhiteSpace(request.BillingAddress) ||
            request.Items == null || !request.Items.Any())
        {
            throw new InvalidOrderDataException("Datos incompletos para la orden.");
        }

        var customer = await _repository.GetById<Customer>(request.CustomerId);
        if (customer == null)
        {
            throw new CustomerNotFoundException(request.CustomerId);
        }

        var orderItems = new List<OrderItem>();
        decimal total = 0m;

        foreach (var item in request.Items)
        {
            var product = await _repository.GetById<Product>(item.ProductId);

            if (product == null || !product.IsActive)
            {
                throw new ProductNotFoundException(item.ProductId);
            }

            if (product.StockQuantity < item.Quantity)
            {
                throw new InsufficientStockException(product.Name, product.StockQuantity, item.Quantity);
            }

            product.StockQuantity -= item.Quantity;
            await _repository.Update(product);

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.CurrentUnitPrice
            };

            orderItems.Add(orderItem);
            total += orderItem.Subtotal;
        }

        var order = new Order(request.CustomerId, request.ShippingAddress, request.BillingAddress, null)
        {
            TotalAmount = total,
            OrderItems = orderItems
        };

        await _repository.Add(order);

        var orderItemResponses = orderItems.Select(oi => new OrderItemResponse(
            oi.ProductId ?? Guid.Empty,
            oi.Product?.Name ?? "(sin nombre)",
            oi.Quantity,
            oi.UnitPrice,
            oi.Subtotal
        )).ToList();

        return new OrderResponse(
            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.TotalAmount,
            order.Status,
            orderItemResponses
        );

        }

    //Traer Todas Las Ordenes

    public async Task<IEnumerable<OrderResponse>> GetOrders(

            OrderStatus? status,
            Guid? CustomerId,
            int pageNumber,
            int pageSize

         )
    {

        var query = await _repository.GetAll<Order>("OrderItems.Product"); //Obtenemos TOdas las Ordenes cargadas en la db

        if (status.HasValue) //Si pasamos un estado Filtramos por ese Estado
            query = query.Where(o => o.Status == status.Value);

        if(CustomerId.HasValue) //Si pasamos El Id del Customer, Filtramos Por ese id
            query = query.Where(o => o.CustomerId == CustomerId.Value);

        
    var paginated = query
            .Skip((pageNumber-1) * pageSize) //Salta Las ordenes de las Paginas Anteriores
            .Take(pageSize) // Take es un entero quen Toma la Cantidad de ordenes que le queremos pasar
            .ToList(); //Almacenamos las Ordenes en una Lista

        return paginated.Select(order => new OrderResponse( 
     order.Id,
            order.Date,
         order.ShippingAddress,
        order.BillingAddress,
        order.Notes,
         order.TotalAmount,
            order.Status,
  order.OrderItems.Select(oi => new OrderItemResponse(
      oi.ProductId ?? Guid.Empty,
      oi.Product?.Name ?? string.Empty,
      oi.Quantity,
      oi.UnitPrice,
      oi.Subtotal)).ToList()
));





    }


   

    //Buscar una Orden Por su ID

    public async Task<OrderResponse?> GetOrderById(Guid id) 
    {
    
    var order = await _repository.GetById<Order>( id, "OrderItems.Product" );

        if (order == null) return null;

        var orderItemResponses = order.OrderItems.Select(oi => new OrderItemResponse(

            oi.ProductId ?? Guid.Empty,
            oi.Product.Name,
            oi.Quantity,
            oi.UnitPrice,
            oi.Subtotal


        )).ToList(); //Transformamos Los items de la orden en una Lista

        return new OrderResponse( //Devolvemos la Orden


            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.TotalAmount,
            order.Status,
            orderItemResponses
            );



    }

    //Actualizar el Estado de una orden

    public async Task<OrderResponse> UpdateOrderStatus(Guid orderId, string newStatus)
    {
        var order = await _repository.GetById<Order>(orderId, nameof(Order.OrderItems), $"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}");

        if (order == null)
        {
            throw new OrderNotFoundException(orderId);
        }

        if (newStatus.Any(char.IsDigit))
        {
            throw new InvalidOrderStatusException($"El estado '{newStatus}' no puede contener números. Por favor, use uno de los siguientes: {string.Join(", ", Enum.GetNames(typeof(OrderStatus)))}");
        }
        if (!Enum.TryParse(newStatus, true, out OrderStatus parsedStatus) || !Enum.IsDefined(typeof(OrderStatus), parsedStatus))
        {
            throw new InvalidOrderStatusException($"El estado '{newStatus}' no es un valor válido. Los valores permitidos son: {string.Join(", ", Enum.GetNames(typeof(OrderStatus)))}");
        }
        order.Status = parsedStatus;

        var updatedOrder = await _repository.Update(order);

        return new OrderResponse(
            updatedOrder.Id,
            updatedOrder.Date,
            updatedOrder.ShippingAddress,
            updatedOrder.BillingAddress,
            updatedOrder.Notes,
            updatedOrder.TotalAmount,
            updatedOrder.Status,
            updatedOrder.OrderItems.Select(oi => new OrderItemResponse(
                oi.ProductId ?? Guid.Empty,
                oi.Product?.Name ?? "(sin nombre)",
                oi.Quantity,
                oi.UnitPrice,
                oi.Subtotal)).ToList()
        );
    }


    















}







    
