using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/products")]
    public class ProductControllerr : ControllerBase
    {
        private readonly ProductsManagementService _service;

        public ProductControllerr(ProductsManagementService service)
        {
            _service = service;
        }

        [HttpPost] //CREAMOS UN PRODUCTO
        public async Task<IActionResult> CreateProduct([FromBody] ProductModel.Request request) //en el FromBody indicamos que los Datos Provienen del cuerpo Del Json
        {
            var productResponse = await _service.AddProduct(request); //Llamamos al metodo q contiene la logica
            return CreatedAtAction(nameof(GetProductById), new { id = productResponse.id }, productResponse);
        }

        [HttpGet] //TRAEMOS TODOS LOS PRODUCTOS
        public async Task<IActionResult> GetAvaibleProduct()
        {
            var products = await _service.GetProducts();
            if (products == null || !products.Any()) return NotFound();
            return Ok(products);
        }

        [HttpGet("{Id}")] //TRAEMOS LOS PRODUCTOS POR SU ID
        public async Task<IActionResult> GetProductById(Guid Id)
        {
            var product = await _service.GetProductById(Id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPut("{id}")] //ACTUALIZAR UN PRODUCTO
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromBody] ProductModel.UpdateRequest request)
        {
            if (request == null) return BadRequest("Los Datos Del producto No pueden Estar Vacios");

            var updatedProduct = await _service.UpdateProduct(id, request);
            if (updatedProduct == null) return NotFound($"No se encontró un producto con el ID {id}");

            return Ok(updatedProduct);
        }

        [HttpPatch("{id}")]  //DAR DE BAJA UN PRODUCTO
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            await _service.DisableProduct(id);
            return NoContent();
        }
    }
}

