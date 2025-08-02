using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public  class Product : EntityBase
    {
        public Product()
        {

        }
        public Product(string? sku, string? internalCode , string? name, string? description , decimal currentUnitPrice, int stockQuantity, bool isActive = true )
        {
            Sku = sku;
            Name = name;
            InternalCode = internalCode;
            Description = description;
            CurrentUnitPrice = currentUnitPrice;
            StockQuantity = stockQuantity;
            IsActive = true;
        }

        public String? Sku { get; set; }
     public String? InternalCode { get; set; }
     public String? Name { get; set; }
     public String? Description { get; set; }    
     public Decimal CurrentUnitPrice { get; set; }   
     public int StockQuantity { get; set; }
     public bool IsActive { get; set; }   
     
     // todos los ordersItems que usan este producto   
     public ICollection<OrderItem> OrderItems { get; set; }
    
    
    
    }
    






}
