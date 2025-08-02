using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    [Table("Customer")]
    public  class Customer : EntityBase
    {
        public Customer()
        {
            
        }

        public Customer(string? email, string? name, string? phoneNumber)
        {
            eMail = email;
            Name = name;
            PhoneNumber = phoneNumber;
        }

        public String? eMail {  get; set; }

        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }


    }
}
