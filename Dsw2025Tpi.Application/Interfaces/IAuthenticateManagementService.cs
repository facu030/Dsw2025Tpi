using Dsw2025Tpi.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Interfaces
{
    public interface IAuthenticateManagementService
    {
        Task<LoginResponse> Login(LoginModel request);
        Task<RegisterResponse> Register(RegisterModel request);

        Task ChangeUserRole(string userId, string role);
    }
}
