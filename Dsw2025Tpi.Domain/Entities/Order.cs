using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Order : EntityBase
    {
        public Order()
        {

        }
        public Order(string shippingAddress, string billingAddress, string notes, List<OrderItem> orderItems, decimal totalAmount, Guid customerId)
        {
            Date = DateTime.Now;
            ShippingAddress = shippingAddress;
            BillingAddress = billingAddress;
            Notes = notes;
            OrderItems = orderItems ?? new List<OrderItem>();
            TotalAmount = totalAmount;
            Status = OrderStatus.PENDING;
            CustomerId = customerId;
        }
        public DateTime Date { get; set; }
        public string ShippingAddress { get; set; } //direccion de entrega del pedido
        public string BillingAddress { get; set; } //direccion de facturacion
        public string Notes { get; set; }
        public decimal TotalAmount { get; set; } 
        public OrderStatus Status { get; set; }

        public Guid CustomerId { get; set; }

        
        public ICollection<OrderItem> OrderItems { get; private set; }

        public Customer Customer { get; set; }
        

    }
}
