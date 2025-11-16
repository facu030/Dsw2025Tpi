using Dsw2025Tpi.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Interfaces
{
    public interface IProductsManagementService
    {
        Task<ProductModel.Response?> GetProductById(Guid id);
        Task<IEnumerable<ProductModel.Response>?> GetProducts();
        Task<ProductModel.Response> AddProduct(ProductModel.ProductRequest request);
        Task<ProductModel.Response> UpdateProduct(Guid id, ProductModel.ProductRequest request);
        Task<ProductModel.Response> DisableProduct(Guid id);
    }
}
