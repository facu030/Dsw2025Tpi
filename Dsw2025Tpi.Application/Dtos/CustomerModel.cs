using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public static class CustomerModel
    {
        public record CreateCustomerRequest(string Name, string Email, string UserId);
        public record CustomerResponse(Guid Id, string Name, string Email, string UserId);


    }
}
