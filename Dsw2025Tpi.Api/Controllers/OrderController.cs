using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly OrderManagementService _service;

        public OrderController(OrderManagementService service)
        {
            _service = service;
        }

        /*PARA CREAR UNA NUEVA ORDEN*/
        [HttpPost()]
        public async Task<IActionResult> AddOrder([FromBody] OrderModel.OrderRequest request)
        {
            try
            {
                var order = await _service.AddOrder(request);
                return Created($"order created/{order.Id}", order);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException iex)
            {
                return BadRequest(iex.Message);
            }
            catch (Exception)
            {
                return Problem("Se produjo un error al crear la Orden");
            }
        }

        /*PARA OBTENER UNA ORDEN POR ID*/
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                var orden = await _service.DetailedOrderById(id);
                return Ok(orden);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return Problem("Se produjo un error buscar la orden requerida");
            }
        }

        /*PARA ACTUALIZAR EL ESTADO DE UNA ORDEN*/
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id,[FromBody] OrderModel.UpdateStatusRequest request)
        {
            try
            {
                var updatedOrder = await _service.UpdateOrderStatus(id, request.status);
                return Ok(updatedOrder);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message); // 404 Not Found
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // 400 Bad Request
            }
            catch (Exception ex)
            {
                return Problem("Se produjo un error buscar al intentar cambiar el estado de la orden");
            }
        }

        /*PARA OBTENER TODAS LAS ORDENES*/
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _service.GetAllOrders();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return Problem("Se produjo un error al intentar obtener las ordenes");
            }
        }

    }
}
