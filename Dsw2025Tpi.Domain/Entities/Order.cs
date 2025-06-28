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
        public Order(string shippingAddress, string billingAddress, string notes, decimal totalAmount, List<OrderItem> orderItems, Guid customerId)
        {
            Status = OrderStatus.PENDING;
            Date = DateTime.Now;
            ShippingAddress = shippingAddress;
            BillingAddress = billingAddress;
            Notes = notes;
            OrderItems = orderItems ?? new List<OrderItem>();
            TotalAmount = totalAmount;
            CustomerId = customerId;
        }
        public OrderStatus Status { get; set; }
        public DateTime Date { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string Notes { get; set; }
        public decimal TotalAmount { get; set; }

        //clave foranea q conecta con cliente
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        //Una Order puede tener muchos OrderItems
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
