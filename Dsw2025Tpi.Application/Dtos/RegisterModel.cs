using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    // Lo que manda el front en /api/auth/register
    public record RegisterModel(string Username, string Email, string Password);

    // Lo que devuelve el servicio de registro
    public record RegisterResponse(string Token, IdentityUser User, string Role);
}