using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos;

public  class ProductModel
{
    public record Request( //datos que el cliente envía para realizar una operación específica

        String Sku,
        string InternalCode,
        String Name,
        String Description,
        decimal CurrentUnitPrice,
        int StockQuantity

        );

    public record Response( //Las entidades que el cliente quiere ver

        Guid id,
        String Sku,
        string InternalCode,
        String Name,
        String Description,
        decimal CurrentUnitPrice,
        int StockQuantity,
        bool IsActive
       

        );

    public record UpdateRequest( //para Poder Actualizar Los productos
        String Sku,
        string InternalCode,
        String Name,
        String Description,
        decimal CurrentUnitPrice,
        int StockQuantity
        );


}
