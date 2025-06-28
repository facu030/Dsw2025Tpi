using System.Linq.Expressions;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;

using ApplicationException = System.ApplicationException;

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

    public async Task<List<ProductModel.Response>?> GetProducts()
    {
        var products = await _repository.GetAll<Product>();

        if (products == null || !products.Any() || products.Where(p => p.IsActive) == null)
        {
            throw new EntityNotFoundException("No se encontraron productos activos.");
        }
        return products?.Select(p => new ProductModel.Response(
            p.Id,
            p.Sku,
            p.InternalCode,
            p.Name,
            p.Description,
            p.CurrentUnitPrice,
            p.StockQuantity,
            p.IsActive
        )).ToList();
    }

    public async Task<ProductModel.Response> AddProduct(ProductModel.Request request)
    {

        if (string.IsNullOrWhiteSpace(request.Sku) ||
            string.IsNullOrWhiteSpace(request.InternalCode) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Description) ||
            request.Stock < 0
            )
        {
            throw new ArgumentException("Valores para el producto no válidos");
        }

        if (request.Price < 0) throw new PriceNullException("El precio del producto no puede ser menor a 0.");
        var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
        if (exist != null) throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");
        if (string.IsNullOrWhiteSpace(request.Sku) || string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("SKU y nombre son obligatorios.");

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
    public async Task<ProductModel.Response> UpdateProductAsync(ProductModel.Request request, Guid id)
    {
        var product = await _repository.GetById<Product>(id);
        if (product == null)
            throw new EntityNotFoundException($"Producto con ID {id} no encontrado.");
        if (request == null ||
                string.IsNullOrWhiteSpace(request.Sku) ||
                string.IsNullOrWhiteSpace(request.InternalCode) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                request.Price <= 0)
            throw new ArgumentException("Valores para el producto no validos");

        product.Sku = request.Sku;
        product.InternalCode = request.InternalCode;
        product.Name = request.Name;
        product.Description = request.Description;
        product.CurrentUnitPrice = request.Price;
        product.StockQuantity = request.Stock;

        await _repository.Update(product);

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
    public async Task<bool> DisableProductAsync(Guid id)
    {
        var product = await _repository.GetById<Product>(id);
        if (product == null || !product.IsActive)
            throw new EntityNotFoundException("Producto no encontrado.");

        product.IsActive = false;
        await _repository.Update(product);
        return true;
    }
}