using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsManagementService _service;

        public ProductsController(IProductsManagementService service)
        {
            _service = service;
        }

        [HttpGet()]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _service.GetProducts();
            if (products == null || !products.Any()) return NoContent();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _service.GetProductById(id);
            if (product == null) return NotFound($"No se encontró producto con el id {id}");
            return Ok(product);
        }

        [HttpPost()]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel.ProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                return BadRequest(firstError);
            }
     
                var product = await _service.AddProduct(request);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
                
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.ProductRequest request)
        {
                var product = await _service.UpdateProduct(id, request);
                return Ok(product);      
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            await _service.DisableProduct(id);
            return NoContent();
        }
    }
}
