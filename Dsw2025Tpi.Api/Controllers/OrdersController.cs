using System.Security.Claims;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[Route("api/orders")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrdersManagementService _service;

    public OrdersController(IOrdersManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _service.GetOrders();
        if (orders == null || !orders.Any()) return NoContent();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _service.GetOrderById(id);
        if (order == null) return NotFound($"No se encontró la orden con el id {id}");
        return Ok(order);
    }

    [HttpGet("admin")]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAuthOrders([FromQuery] OrderModel.FilterOrder request)
    {
        var result = await _service.GetOrders(request);

        if (result == null || result.Total == 0)
        {
            Response.Headers.Append("X-Message", "No se encontraron órdenes");
            return NoContent(); // 204
        }

        return Ok(result); // 200 con OrderItems + Total
    }

    // Endpoint viejo que recibe customerId (lo dejamos para pruebas / admin)
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderModel.OrderRequest request)
    {
        var order = await _service.CreateOrder(request);
        return Ok(order);
    }

    // nuevo end para crear orden para el usuario autenticado (sin customerId en el body)
    // POST: /api/orders/me
    [HttpPost("me")]
    public async Task<IActionResult> CreateOrderForCurrentUser(
        [FromBody] OrderModel.OrderFromUserRequest request)
    {
        //Tomamos el userId desde el token (claim estándar de Identity)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("No se pudo identificar al usuario autenticado.");
        }

        //Delegamos en el servicio que crea la orden asociada a ese usuario
        var order = await _service.CreateOrderForUserAsync(userId, request);

        return Ok(order);
    }
}
