using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Product : EntityBase
    {
        public Product()
        {

        }
        public Product(string sku, string internalCode, string name, string description, decimal price, int stock)
        {
            Sku = sku;
            InternalCode = internalCode;
            Name = name;
            Description = description;
            CurrentUnitPrice = price;
            IsActive = true;
            StockQuantity = stock;
        }

        public string? Sku { get; set; }
        public string? InternalCode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal CurrentUnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }


        public ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();

        public bool StockSufficient(int quantity)
        {
            return StockQuantity >= quantity;
        }

        public void DecreaseStock(int quantity)
        {
            StockQuantity -= quantity;
        }

    }
}
