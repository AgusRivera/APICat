using APICat.Application.Interfaces.Auth;
using APICat.Application.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace APICat.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginDto login)
        {
            var token = _authService.GenerateToken(login.Username, login.Password);

            if (token == null)
            {
                return Unauthorized("Credenciales inválidas. Prueba: admin / 1234");
            }

            return Ok(new { Token = token });
        }
    }
}
