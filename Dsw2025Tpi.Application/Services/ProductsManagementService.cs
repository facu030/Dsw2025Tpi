using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Helpers;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductsManagementService
    {
        private readonly IRepository _productRepository;
        public ProductsManagementService(IRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductModel.Response?> GetProductById(Guid id)
        {
            var product = await _productRepository.GetById<Product>(id);

            return product != null && product.IsActive ?
                new ProductModel.Response(product.Id, product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentUnitPrice, product.StockQuantity) :
                null;
        }

        public async Task<IEnumerable<ProductModel.Response>?> GetProducts()
        {
            var products = await _productRepository.GetFiltered<Product>(p => p.IsActive);
            return products?.Select(p => new ProductModel.Response(p.Id, p.Sku, p.InternalCode, p.Name, p.Description, p.CurrentUnitPrice, p.StockQuantity));
        }

        public async Task<ProductModel.Response> AddProduct(ProductModel.ProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            request.CurrentUnitPrice <= 0
            || request.StockQuantity < 0)
            {
                throw new ArgumentException("Valores para el producto no válidos");
            }

            var exist = await _productRepository.First<Product>(p => p.Sku == request.Sku || p.InternalCode == request.InternalCode);
            if (exist != null) throw new DuplicatedEntityException("Ya existe un producto con el mismo SKU o código interno");
            var product = new Product(request.Sku, request.InternalCode, request.Name, request.Description, request.CurrentUnitPrice, request.StockQuantity);
            await _productRepository.Add(product);
            return new ProductModel.Response(product.Id, product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentUnitPrice, product.StockQuantity);

        }

        public async Task<ProductModel.Response> UpdateProduct(Guid id, ProductModel.ProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.InternalCode) ||
            request.CurrentUnitPrice < 0 || request.StockQuantity < 0)
            {
                throw new ArgumentException("Valores para el producto no válidos");
            }

            var product = await _productRepository.GetById<Product>(id);
            if (product == null)  throw new EntityNotFoundException("No existe un producto con el Id especificado");
            
            if (ProductComparer.HasChanges(product, request))
            {
                product.Sku = request.Sku;
                product.InternalCode = request.InternalCode;
                product.Name = request.Name;
                product.Description = request.Description;
                product.CurrentUnitPrice = request.CurrentUnitPrice;
                product.StockQuantity = request.StockQuantity;
                await _productRepository.Update(product);
                return new ProductModel.Response(product.Id, product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentUnitPrice, product.StockQuantity);
            }
            throw new ArgumentException("No se han modificado los valores del producto");

        }

        public async Task<ProductModel.Response> DisableProduct(Guid id)
        {
            var product = await _productRepository.GetById<Product>(id);
            if (product == null || !product.IsActive) throw new EntityNotFoundException("No existe un producto con el ID especificado");
            product.IsActive = false;
            await _productRepository.Update(product);
            return new ProductModel.Response(product.Id, product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentUnitPrice, product.StockQuantity);

        }
    }
}
