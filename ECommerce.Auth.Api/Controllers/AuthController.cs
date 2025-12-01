// ECommerce.Auth.Api/Controllers/AuthController.cs

using Auth.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers
{
    // DTO de entrada para login
    public class LoginRequest
    {
        public string Usuario { get; set; } = null!;
        public string Senha { get; set; } = null!;
    }
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResult>> Login([FromBody] LoginRequest request)
        {
            // Lógica de autenticação Simples (Em produção, aqui você checaria o banco de dados)
            if (request.Usuario == "teste" && request.Senha == "123456")
            {
                // Gera o token
                var token = await _authService.GenerateToken(request.Usuario);
                return Ok(token);
            }

            return Unauthorized("Credenciais inválidas.");
        }
    }
}