using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Customer : EntityBase
    {
        public Customer() { }

        public Customer(string name, string email, string? phoneNumber = null)
        {
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
        }

        // Usa el Id de EntityBase

        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }

        public string? UserId { get; set; }

        [NotMapped]
        public IdentityUser? User { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}