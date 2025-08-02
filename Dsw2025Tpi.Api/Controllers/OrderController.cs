using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/controller")] // Se mantiene tu ruta original
    public class OrderController : ControllerBase
    {
        private readonly OrderManagementService _orderservice;

        public OrderController(OrderManagementService orderservice)
        {
            _orderservice = orderservice;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            var created = await _orderservice.CreateOrder(request);
            return CreatedAtAction(nameof(GetOrderById), new { id = created.OrderId }, created);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] OrderStatus status, // Se mantienen tus parámetros originales
            [FromQuery] Guid customerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var orders = await _orderservice.GetOrders(status, customerId, pageNumber, pageSize);
            if (orders == null || !orders.Any()) return NoContent();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderservice.GetOrderById(id);
            if (order == null) return NotFound($"No se encontró una orden con el ID: {id}");
            return Ok(order);
        }

        //Modificar EL estado de una Orden

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var updatedOrder = await _orderservice.UpdateOrderStatus(id, request.NewStatus.ToString());
            return Ok(updatedOrder);
        }
    }
}