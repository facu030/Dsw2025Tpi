using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Dsw2025Tpi.Application.Services
{
    public class AuthenticateManagementService : IAuthenticateManagementService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ICustomersManagementService _customersManagementService;

        public AuthenticateManagementService(
            UserManager<IdentityUser> userManager,
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

        public async Task<LoginResponse> Login(LoginModel request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);

            if (user == null)
                throw new EntityNotFoundException("Usuario o contraseña incorrecta");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                throw new EntityNotFoundException("Usuario o contraseña incorrecta");

            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() ?? "User";

            var token = _jwtTokenService.GenerateToken(user.UserName!, roleName);

            return new LoginResponse(
                Token: token,
                User: user,
                Role: roleName
            );
        }

        public async Task<RegisterResponse> Register(RegisterModel request)
        {
            // ¿ya existe el username?
            var userExists = await _userManager.FindByNameAsync(request.Username);
            if (userExists != null)
                throw new DuplicatedEntityException("El usuario ya existe");

            // ¿es el primer usuario del sistema?
            var isFirstUser = !_userManager.Users.Any();

            // primer user → Admin, resto → User
            var roleName = isFirstUser ? "Admin" : "User";

            // aseguramos que el rol exista (aunque ya lo seeds en IdentitySeederExtensions)
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole(roleName));

            var user = new IdentityUser
            {
                UserName = request.Username,
                Email = request.Email
            };

            var userResult = await _userManager.CreateAsync(user, request.Password);

            if (!userResult.Succeeded)
                throw new ArgumentException(string.Join(", ",
                    userResult.Errors.Select(e => e.Description)));

            var roleResult = await _userManager.AddToRoleAsync(user, roleName);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                throw new ArgumentException("No se pudo asignar el rol.");
            }

            try
            {
                // si es un usuario normal, lo consideramos cliente
                if (roleName == "User")
                {
                    var customerModel = new CustomerModel.CreateCustomerRequest(
                        user.UserName!,
                        user.Email!,
                        user.Id
                    );

                    await _customersManagementService.CreateCustomer(customerModel);
                }
            }
            catch
            {
                // si falla crear el customer, borramos el user para no dejar datos colgados
                await _userManager.DeleteAsync(user);
                throw;
            }

            var token = _jwtTokenService.GenerateToken(user.UserName!, roleName);

            return new RegisterResponse(
                Token: token,
                User: user,
                Role: roleName
            );
        }
        public async Task ChangeUserRole(string userId, string role)
        {
            // Validar que el rol sea uno de los permitidos
            if (role != "Admin" && role != "User")
                throw new ArgumentException("Rol inválido. Solo se permite 'Admin' o 'User'.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("Usuario no encontrado");

            // Asegurar que el rol exista
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(role));
                if (!createRoleResult.Succeeded)
                    throw new ArgumentException("No se pudo crear el rol especificado.");
            }

            // Sacar todos los roles actuales
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    throw new ArgumentException("No se pudieron quitar los roles actuales del usuario.");
            }

            // Agregar el nuevo rol
            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
                throw new ArgumentException("No se pudo asignar el nuevo rol al usuario.");
        }
    }
}