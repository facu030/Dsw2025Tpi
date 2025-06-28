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
        public OrderItem(int quantity, decimal unitePrice, decimal subTotal, Guid orderId, Guid productId)
        {
            Quantity = quantity;
            UnitPrice = unitePrice;
            Subtotal = subTotal;
            OrderId = orderId;
            ProductId = productId;
        }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        //relacion con orden y producto por id obtengo sus claves foraneas
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public decimal CalcularSubTotal() => UnitPrice*Quantity;
    }
}
