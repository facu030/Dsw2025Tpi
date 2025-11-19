using Dsw2025Tpi.Application.Dtos;
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

        public AuthenticateController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            JwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
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
            {
                return BadRequest("El usuario ya existe");
            }
            var user = new IdentityUser
            {
                UserName = request.Username,
                Email = request.Email,

            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest("Error al crear el usuario");
            }

            if (!await _roleManager.RoleExistsAsync(request.Role))
            {
                return BadRequest($"El rol '{request.Role}' no existe.");
            }

            await _userManager.AddToRoleAsync(user, request.Role);

            return Ok("Usuario creado exitosamente");
        }
    }
}
