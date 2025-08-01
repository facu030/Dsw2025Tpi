
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exeptions;
using Dsw2025Tpi.Domain.Entities;



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
        [HttpPost()]
        /*PARA CREAR UN NUEVO PRODUCTO*/
        [HttpPost()]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel.Request request)
        {
            try
            {
                var product = await _service.AddProduct(request);
                return Created($"product created/{product.Id}", product);
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
    }
}
