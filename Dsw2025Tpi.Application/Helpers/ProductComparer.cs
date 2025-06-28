using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Helpers
{
    public class ProductComparer
    {
        public static bool HasChanges(Product product, ProductModel.ProductRequest request)
        {
            return product.Sku != request.Sku ||
                   product.InternalCode != request.InternalCode ||
                   product.Name != request.Name ||
                   product.Description != request.Description ||
                   product.CurrentUnitPrice != request.CurrentUnitPrice ||
                   product.StockQuantity != request.StockQuantity;
        }
    }
}
