using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsManagementService _service;

        public ProductsController(ProductsManagementService service)
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
            try
            {
                var product = await _service.AddProduct(request);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (DbUpdateException)
            {
                return BadRequest("Error al actualizar la base de datos");
            }
            catch (DuplicatedEntityException de)
            {
                return BadRequest(de.Message);
            }
            catch (Exception)
            {
                return Problem("Se produjo un error al guardar el producto");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.ProductRequest request)
        {
            try
            {
                var product = await _service.UpdateProduct(id, request);
                return Ok(product);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (DbUpdateException)
            {
                return BadRequest("Error al actualizar la base de datos");
            }
            catch (EntityNotFoundException nf)
            {
                return NotFound(nf.Message);
            }
            catch (Exception)
            {
                return Problem("Se produjo un error al actualizar el producto");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            try
            {
                await _service.DisableProduct(id);
                return NoContent();
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (EntityNotFoundException nf)
            {
                return NotFound(nf.Message);
            }
            catch (Exception)
            {
                return Problem("Se produjo un error al deshabilitar el producto");
            }
        }
    }
}
