using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return product != null ?
                new ProductModel.Response(product.Id, product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentUnitPrice, product.StockQuantity) :
                null;
        }

        public async Task<IEnumerable<ProductModel.Response>?> GetProducts()
        {
            var products = await _productRepository.GetFiltered<Product>(p => p.IsActive);
            return products?.Select(p => new ProductModel.Response(p.Id, p.Sku, p.InternalCode, p.Name, p.Description, p.CurrentUnitPrice, p.StockQuantity));
        }

        public async Task<ProductModel.Response> AddProduct(ProductModel.Request request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            request.CurrentUnitPrice < 0
            || request.StockQuantity < 0)
            {
                throw new ArgumentException("Valores para el producto no válidos");
            }
            
            var exist = await _productRepository.First<Product>(p => p.Sku == request.Sku || p.InternalCode == request.InternalCode);
            if (exist != null) throw new ArgumentException("Ya existe un producto con el mismo SKU o código interno");
            var product = new Product(request.Sku, request.InternalCode, request.Name, request.Description, request.CurrentUnitPrice, request.StockQuantity);
            await _productRepository.Add(product);
            return new ProductModel.Response(product.Id, product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentUnitPrice, product.StockQuantity);

        }

    }
}
