using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Customer : EntityBase
    {
        public Customer(string Name, string Email){
            this.Name = Name;
            this.Email = Email;
        }

        public new Guid Id { get; protected set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string UserId { get; set; }
        [NotMapped]
        public IdentityUser User { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
