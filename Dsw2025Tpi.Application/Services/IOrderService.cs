using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;

namespace Dsw2025Tpi.Application.Services
{
    public interface IOrderService
    {
        Task<OrderModel.OrderResponse> AddOrder(OrderModel.OrderRequest request);
    }
}
