using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public record ProductModel
    {
        public record Request(
            string Sku,
            string InternalCode,
            string Name,
            string Description,
            decimal Price,
            int Stock
        );
        public record ProductRequest(string Sku, string Name, decimal Price, string InternalCode, string Descripcion, int Stock);
        public record ProductResponse(Guid Id, string Sku, string Name, decimal Price, string InternalCode, string Descripcion, int Stock);
        public record Response(
            Guid Id,
            string? Sku,
            string? InternalCode,
            string Name,
            string Description,
            decimal Price,
            int Stock,
            bool IsActive
        );
    }
}
