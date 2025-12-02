using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    [HttpGet()]
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
    // Si quisieras restringir solo a admins en algún momento:
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

    [HttpPost()]
    public async Task<IActionResult> CreateOrder([FromBody] OrderModel.OrderRequest request)
    {

            var order = await _service.CreateOrder(request);
            return Ok(order);
       
    }

}