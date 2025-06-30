using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Customer : EntityBase
    {
        public Customer(string Name, string Email, string PhoneNumber){
            this.Name = Name;
            this.Email = Email;
            this.PhoneNumber = PhoneNumber;
        }

        public new Guid Id { get; protected set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
