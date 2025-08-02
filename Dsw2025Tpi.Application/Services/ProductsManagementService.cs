using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Application.Services;
//aqui realizaremos las operaciones para el Controlador
public  class ProductsManagementService
{
    private readonly IRepository _repository;

    public ProductsManagementService(IRepository repository)
	{
       _repository = repository;
    }

    public async Task<ProductModel.Response> AddProduct(ProductModel.Request request) {

        //validaciones  

        if (string.IsNullOrWhiteSpace(request.Sku))  //Validamos que el valor ingresado para el Sku no sea nulo o con espacios
            throw new ArgumentException("El Sku Es obligatorio"); //si falla lanza esta excepcion

        if (string.IsNullOrWhiteSpace(request.Name))  //Validamos que el nombre del Producto no  este vacio o con espacios 
            throw new ArgumentException("Nombre es Obligatorio"); //si falla lanza esta excepcion

        if (request.CurrentUnitPrice <= 0) //Validamos que el Precio del producto No sea negativo 
            throw new ArgumentException("El Precio Debe ser Mayor que Cero"); //si falla lanza una excepcion

        if (request.StockQuantity < 0) //Valdamos que el Stock no sea menor que cero
            throw new ArgumentException("Stock no Puede ser Negativo"); //si falla lanza esta excepcion
         
        //Verificamos si ya existe un producto con el Mismo SKu

        var  existing = await _repository.First<Product>(p  => p.Sku == request.Sku); //Consulta al repositorio si ya existe un producto con ese SKU.El método First recibe una expresión lambda para filtrar
        if (existing != null)
            throw new DuplicatedEntityException($"Ya existe un producto con SKU {request.Sku}");
    
        //crear Entidad Product

        var product = new Product(
            
             request.Sku,
           request.InternalCode,
             request.Name,
          request.Description,
            request.CurrentUnitPrice,
            request.StockQuantity
            );
    //Guardamos en la Bd

       await _repository.Add(product);

        //Retornamos DTO de Respuesta
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

    //Traer Todos los productos activos

    public async Task<IEnumerable<ProductModel.Response>> GetProducts() {


        return ( await _repository
            .GetFiltered<Product>(p => p.IsActive)) //le pedimos al Repo que solo traiga los productos activos, el Get Filtered se utiliza para decirle al codigo que filtre
            .Select(p => new ProductModel.Response  //A cada producto recuperado lo transformás en un ProductModel.Response, que es el modelo que vos retornás en el endpoint.
            (p.Id,
                p.Sku,
                p.InternalCode,
                p.Name,
                p.Description,
                p.CurrentUnitPrice,
                p.StockQuantity,
                p.IsActive
            ));




    }


    //Buscar Producto Por Su ID

    public async Task<ProductModel.Response?> GetProductById(Guid id)
    {
        var product = await _repository.GetById<Product>(id); //Llamamos al Repo para buscar en la Db el id indicado

        if (product == null) // Verificamos si el producto es nulo
            throw new EntityNotFoundException($"No se encontró un producto con el ID {id}"); //Mostramos Este mensaje de error

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

     //Actualizar un Producto Por Su Id

        public async Task<ProductModel.Response?> UpdateProduct(Guid id, ProductModel.UpdateRequest request) 
        { 
    
        var product = await _repository.GetById<Product>(id); //Buscamos el producto Por su id

        if (product == null) return null; //Verificamos si el producto no es nulo osea no esta vacio

        if (string.IsNullOrWhiteSpace(request.Sku) || //verificamos que el Sku no este vacio o con espacio
            string.IsNullOrWhiteSpace(request.Name) || //verificamos que el Nombre no este vacio o con espacio
            request.CurrentUnitPrice <= 0 || request.StockQuantity < 0) //Verificamos que  ni el precio ni el stock sea negativo

        {
            throw new ArgumentException("Datos invalidos Para El Producto"); 

        }

        //Validamos Que el SKu no Este Repetido

        var otherWithSameSku = await _repository.First<Product>( //Busca cualquier producto con ese Sku excluyendo el nuevo
            p => p.Sku == request.Sku && p.Id != id);

        if (otherWithSameSku != null)
        {
            throw new DuplicatedEntityException($"Ya existe otro producto con SKU {request.Sku}");
        }

        //Actualizamos Los Campos

        product.Sku = request.Sku;
        product.InternalCode = request.InternalCode;
        product.Name = request.Name;
        product.Description = request.Description;
        product.CurrentUnitPrice = request.CurrentUnitPrice;
        product.StockQuantity = request.StockQuantity;

        //Guardamos los Cambios en La db

        var updatedProduct = await _repository.Update(product);

        //Retornamos La respuesta

        return new ProductModel.Response(
        
        updatedProduct.Id,
         updatedProduct.Sku,
         updatedProduct.InternalCode,
         updatedProduct.Name,
         updatedProduct.Description,
         updatedProduct.CurrentUnitPrice,
         updatedProduct.StockQuantity,
         updatedProduct.IsActive
         );


        }

    //Desabilitar Un Producto

    public async Task DisableProduct(Guid id) 
    {
    
    var product = await _repository.GetById<Product>(id);

        if (product == null)
            throw new EntityNotFoundException($"No se encontró un producto con el ID {id}");
    
    product.IsActive = false; 

        await _repository.Update(product);

    }
    




}



