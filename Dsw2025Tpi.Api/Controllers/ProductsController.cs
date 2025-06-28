using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using ApplicationException = System.ApplicationException;


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
        [HttpPost]
        [ProducesResponseType(typeof(ProductModel.ProductResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductModel.ProductResponse>> AddProduct([FromBody] ProductModel.Request request)
        {
            var response = await _services.AddProduct(request);
            return CreatedAtAction(nameof(AddProduct), new { id = response.Id }, response);
        }

        // GET
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductModel.ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<List<ProductModel.ProductResponse>>> GetProducts()
        {
            var products = await _services.GetProducts();
            if (products == null || !products.Any())
            {
                return NoContent();
            }
            return Ok(products);
        }

        // GET por id
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProductModel.ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductModel.ProductResponse>> GetProductById(Guid id)
        {
            var product = await _services.GetProductById(id);
            return Ok(product);
        }

        // PUT
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ProductModel.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductModel.Response>> UpdateProduct([FromRoute] Guid id, [FromBody] ProductModel.Request request)
        {
            var updatedProduct = await _services.UpdateProductAsync(request, id);
            return Ok(updatedProduct);
        }

        // PATCH
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            await _services.DisableProductAsync(id);
            return NoContent();
        }
    }
}