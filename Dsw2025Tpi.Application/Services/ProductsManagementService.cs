using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Application.Services;

public class ProductsManagementServices
{
    private readonly IRepository _repository; //instancia del repositorio (inyección de dependencias)

    // Constructor que recibe la dependencia IRepository
    public ProductsManagementServices(IRepository repository)

    {
        _repository = repository;
    }
    //obtener un producto específico por su ID
    public async Task<ProductModel.Response?> GetProductById(Guid id)
    {
        var product = await _repository.GetById<Product>(id);
        if (product == null)
            return null;

        return new ProductModel.Response(
            product.Id,
            product.Sku,
            product.InternalCode,
            product.Name,
            product.Description,
            product.CurrentUnitPrice,
            product.StockQuantity,
            product.IsActive
        );
    }

    //obtener todos los productos disponibles
    public async Task<IEnumerable<ProductModel.Response>?> GetProducts()
    {
        var products = await _repository.GetFiltered<Product>(p => p.IsActive);
        return products?.Select(p => new ProductModel.Response(
            p.Id,
            p.Sku,
            p.InternalCode,
            p.Name,
            p.Description,
            p.CurrentUnitPrice,
            p.StockQuantity,
            p.IsActive
        ));
    }

    public async Task<ProductModel.Response> AddProduct(ProductModel.Request request)
    {
        //Validación de datos de entrada
        if (string.IsNullOrWhiteSpace(request.Sku) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            request.Price < 0 ||
            request.Stock < 0)
        {
            throw new ArgumentException("Valores para el producto no válidos");
        }
        //Verificación de unicidad del SKU
        var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
        if (exist != null) throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");

        //Creación de la entidad Product
        var product = new Product(request.Sku, request.InternalCode, request.Name, request.Description, request.Price, request.Stock);
        await _repository.Add(product);
        return new ProductModel.Response(
           product.Id,
           product.Sku,
           product.InternalCode,
           product.Name,
           product.Description,
           product.CurrentUnitPrice,
           product.StockQuantity,
           product.IsActive
         );
    }
    public async Task<Product?> UpdateProduct(Guid id, ProductModel.Request request)
    {
        var existingProduct = await _repository.GetById<Product>(id);
        if (existingProduct == null) return null;

        // Validar campos
        if (string.IsNullOrWhiteSpace(request.Sku) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            request.Price < 0 ||
            request.Stock < 0)
        {
            throw new ArgumentException("Valores para el producto no válidos");
        }

        // Actualizar campos
        existingProduct.Sku = request.Sku;
        existingProduct.InternalCode = request.InternalCode;
        existingProduct.Name = request.Name;
        existingProduct.Description = request.Description;
        existingProduct.CurrentUnitPrice = request.Price;
        existingProduct.StockQuantity = request.Stock;

        await _repository.Update(existingProduct);
        return existingProduct;
    }
    public async Task<bool> DisableProduct(Guid id)
    {
        var product = await _repository.GetById<Product>(id);
        if (product == null || !product.IsActive) return false;

        product.IsActive = false;
        await _repository.Update(product);
        return true;
    }

}