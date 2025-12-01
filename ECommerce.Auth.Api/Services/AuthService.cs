// ECommerce.Auth.Api/Services/AuthService.cs

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Api.Services
{
    // DTOs para configuração e resposta
    public class JwtConfig
    {
        public string Secret { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ExpiryMinutes { get; set; }
    }
    
    public class TokenResult
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }

    public class AuthService
    {
        private readonly JwtConfig _jwtConfig;

        public AuthService(IOptions<JwtConfig> jwtConfig)
        {
            _jwtConfig = jwtConfig.Value;
        }

        public Task<TokenResult> GenerateToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var expiration = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryMinutes);

            // Define os Claims (dados contidos no token)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Usa ClaimTypes.Name, que o SalesService lerá como ClienteId
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userId) }), 
                Expires = expiration,
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Task.FromResult(new TokenResult
            {
                Token = tokenHandler.WriteToken(token),
                Expiration = expiration
            });
        }
    }
}