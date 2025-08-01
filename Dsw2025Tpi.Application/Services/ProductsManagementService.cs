using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Application.Exeptions;




namespace Dsw2025Tpi.Application.Services
{
    public class ProductsManagementService
    {
        private readonly IRepository _repository;
        public ProductsManagementService(IRepository repository)
        {
            _repository = repository;
        }
        public async Task<ProductModel.Response> AddProduct(ProductModel.Request request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku) ||
                string.IsNullOrWhiteSpace(request.InternalCode) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                request.CurrentUnitPrice <= 0 ||
                request.StockQuantity < 0)
            {
                throw new ArgumentException("Uno o más valores del producto son inválidos.");
            }

            var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
            if (exist != null) throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");
            var product = new Product(request.Sku, request.InternalCode, request.Name, request.Description, request.CurrentUnitPrice, request.StockQuantity); //creo el producto
            await _repository.Add(product);
            return new ProductModel.Response(product.Id, product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentUnitPrice, product.StockQuantity);
        }



        //el controlador llama a este metodo ,que es el sericio tiene que ir a la interfaz irepositorio que se comunica con entitro framwork qeu tiene la base de datos 
        // y los devuelve como un produc model (DTO) deuvle los productos activos 
        public async Task<IEnumerable<ProductModel.Response>?> GetProducts()
        {
            return (await _repository
                .GetFiltered<Product>(p => p.IsActive))?
                .Select(p => new ProductModel.Response(p.Id, p.Sku, p.InternalCode, p.Name, p.Description,
                p.CurrentUnitPrice, p.StockQuantity));
        }


        //para obtener el producto por el id , me flata escribir la explicacion del codigo 
        public async Task<ProductModel.Response?> GetProductById(Guid id)
        {
            var product = await _repository.GetById<Product>(id);
            return product != null ?
                new ProductModel.Response(product.Id, product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentUnitPrice, product.StockQuantity) : null;
            ;
        }
    }



    }

