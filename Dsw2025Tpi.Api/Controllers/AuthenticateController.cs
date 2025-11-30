using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticateController: ControllerBase
    {
        private readonly IAuthenticateManagementService _authenticateService;

        public AuthenticateController(IAuthenticateManagementService service)
        {
            _authenticateService = service;
        }

        public class ChangeRoleRequest
        {
            public string Role { get; set; } = null!;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            var data = await _authenticateService.Login(request);

            var userNormalized = new { data.User.UserName, data.Role };

            return Ok( new {token = data.Token, user = userNormalized});


        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel request)
        {
            var data = await _authenticateService.Register(request);

            var userNormalized = new { data.User.UserName, data.Role };

            return Ok(new { token = data.Token, user = userNormalized });

        }

        [Authorize(Roles = "Admin")]
        [HttpPut("users/{userId}/role")]
        public async Task<IActionResult> ChangeRole(string userId, [FromBody] ChangeRoleRequest request)
        {
            await _authenticateService.ChangeUserRole(userId, request.Role);
            return Ok();
        }
    }
}
