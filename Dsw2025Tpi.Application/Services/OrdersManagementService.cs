using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class OrdersManagementService
    {
        public readonly IRepository _orderRepository;
        public OrdersManagementService(IRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        //public async Task<IEnumerable<OrderModel.Response>?> GetOrders()
        //{
        //    var orders = await _orderRepository.GetFiltered<Order>(o => o.IsActive);
        //    return orders?.Select(o => new OrderModel.Response(o.Id, o.OrderNumber, o.CustomerName, o.TotalAmount, o.OrderDate));
        //}

        public async Task<OrderModel.Response?> GetOrderById(Guid id)
        {
            var order = await _orderRepository.GetById<Order>(id);
            return order != null ?
                new OrderModel.Response(order.Id, order.CustomerId, order.ShippingAddress, order.BillingAddress, order.TotalAmount, order.Date, order.OrderItems) :
                null;
        }

        public async Task<OrderModel.Response?> CreateOrder(OrderModel.OrderRequest request)
        {
            var order = new Order { CustomerId = request.CustomerId, ShippingAddress = request.ShippingAddress, BillingAddress = request.BillingAddress, OrderItems = request.OrderItems.Select(item => new OrderItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.UnitPrice, }).ToList() };
            var createdOrder = await _orderRepository.Add(order);
            return new OrderModel.Response(createdOrder.Id, createdOrder.CustomerId, createdOrder.ShippingAddress, createdOrder.BillingAddress, createdOrder.TotalAmount,createdOrder.Date , createdOrder.OrderItems);
        }


    }
}
