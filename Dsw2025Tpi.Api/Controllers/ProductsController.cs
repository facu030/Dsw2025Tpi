
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
        /*PARA CREAR UN NUEVO PRODUCTO ENDOPOINT 1 */
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
        /*PARA TRAER TODOS LOS PRODUCTOS  (ENDOPOINT 2) */
        /*PARA OBTENER TODOS LOS PRODUCTOS*/
        [HttpGet()] // RESPONDE UNA SOLICITUD GET DE LARUTA DEL CONTROLADOR 


        //EL METODO DEVUELVE UN IActionRsult lo que me permite devolver los estados ( return ok) 
        public async Task<IActionResult> GetProducts()
        {

            //llamo al metodo  getproducts del servicio que me devuelve la lista  DTos de productos
            var products = await _service.GetProducts();

            //si no se encuentran los productos devuelve 204 ,any esp ara listas vacias
            if (products == null || !products.Any()) return NoContent();

            // si hay productos devueleve 200
            return Ok(products);
        }


        /*PARA OBTENER UN PRODUCTO CON UN ID PARTICULAR  */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductBySku(Guid id)
        {
            var product = await _service.GetProductById(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

    }
}
