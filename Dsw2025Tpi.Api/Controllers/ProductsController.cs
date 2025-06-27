using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;


namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsManagementServices _services;

        public ProductsController(ProductsManagementServices services)
        {
            _services = services;
        }

        // POST
        [HttpPost()]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel.Request request)
        {
            try
            {
                var product = await _services.AddProduct(request);
                return Ok(product);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (DuplicatedEntityException de)
            {
                return Conflict(de.Message);
            }
            catch (Exception)
            {
                return Problem("Se produjo un error al guardar el producto");
            }
        }

        // GET
        [HttpGet()]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _services.GetProducts();
            if (products == null || !products.Any())
                return NoContent();

            return Ok(products);
        }

        // GET
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _services.GetProductById(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.Request updatedProduct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _services.UpdateProduct(id, updatedProduct);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (Exception)
            {
                return Problem("Se produjo un error al actualizar el producto");
            }
        }

        // PATCH
        [HttpPatch("{id}")]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            var success = await _services.DisableProduct(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}