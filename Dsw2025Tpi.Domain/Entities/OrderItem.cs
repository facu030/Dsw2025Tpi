using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class OrderItem : EntityBase
    {   
        public OrderItem()
        {

        }
        public OrderItem(int quantity, decimal unitPrice, decimal subTotal, Guid orderId, Guid productId)
        {
            Quantity = quantity;
            UnitPrice = unitPrice;
            SubTotal = subTotal;
            OrderId = orderId;
            ProductId = productId;
        }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }

        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        
        public Order Order { get; set; }
        public Product Product { get; set; }

        public decimal CalculateSubTotal() => UnitPrice * Quantity;


    }
}
