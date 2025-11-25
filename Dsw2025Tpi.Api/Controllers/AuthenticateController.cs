using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticateController: ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ICustomersManagementService _customersManagementService;

        public AuthenticateController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            JwtTokenService jwtTokenService,
            ICustomersManagementService customersManagementService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _customersManagementService = customersManagementService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return Unauthorized("Usuario o contraseña incorrecta");
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
               return Unauthorized("Usuario o contraseña incorrecta");
            }
            var role = await _roleManager.FindByNameAsync((await _userManager.GetRolesAsync(user)).First());


            var token = _jwtTokenService.GenerateToken(user.UserName, role.Name);
            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel request)
        {
            var userExists = await _userManager.FindByNameAsync(request.Username);
            if (userExists != null)
                return BadRequest("El usuario ya existe");

            var roleName = string.IsNullOrWhiteSpace(request.Role) ? "Customer" : request.Role;

            if (!await _roleManager.RoleExistsAsync(roleName))
                return BadRequest($"El rol '{roleName}' no existe.");

            var user = new IdentityUser
            {
                UserName = request.Username,
                Email = request.Email
            };

            var userResult = await _userManager.CreateAsync(user, request.Password);

            if(!userResult.Succeeded)
                return BadRequest("Error al crear el usuario.");

            var roleResult = await _userManager.AddToRoleAsync(user, roleName);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user); 
                return BadRequest("No se pudo asignar el rol.");
            }

            try
            {
                if (roleName == "Customer")
                {
                    var customerModel = new CustomerModel.CreateCustomerRequest(user.UserName, user.Email, user.Id);

                    var customer = await _customersManagementService.CreateCustomer(customerModel);

                    return Ok(new
                    {
                        Message = "Usuario y cliente creados exitosamente.",
                        Customer = customer
                    });
                }
            }
            catch
            {
                await _userManager.DeleteAsync(user); 
                throw;
            }
            
            return Ok("Usuario creado exitosamente");
        }
    }
}
