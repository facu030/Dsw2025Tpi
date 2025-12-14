using Dsw2025Tpi.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Interfaces
{
    public interface IOrdersManagementService
    {
        Task<IEnumerable<OrderModel.GetResponse>?> GetOrders();
        Task<OrderModel.GetResponse?> GetOrderById(Guid id);

        Task<OrderModel.AddResponse> CreateOrder(OrderModel.OrderRequest request);

        //crear orden para el usuario autenticado (sin CustomerId)
        Task<OrderModel.AddResponse> CreateOrderForUserAsync(
            string userId,
            OrderModel.OrderFromUserRequest request
        );

        //listado con filtros + paginación (para dashboard)
        Task<OrderModel.ResponsePagination> GetOrders(OrderModel.FilterOrder request);
    }
}
