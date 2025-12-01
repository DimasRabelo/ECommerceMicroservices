// ECommerce.Gateway/Program.cs

using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// 1. Carregar a configuração do Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// 2. Configurar a Validação JWT
var authenticationProviderKey = "Bearer";
var jwtConfigSection = builder.Configuration.GetSection("AuthenticationProviderKeys").Get<List<OcelotAuthKey>>()?.FirstOrDefault();
var jwtSecret = jwtConfigSection?.Config.Secret;

if (!string.IsNullOrEmpty(jwtSecret))
{
    builder.Services.AddAuthentication()
        .AddJwtBearer(authenticationProviderKey, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "ECommerce.Auth.Api",
                ValidAudience = "ECommerce.Microservices"
            };
        });
}

// 3. Adicionar Ocelot
builder.Services.AddOcelot();

var app = builder.Build();

// 4. Adicionar Middleware (na ordem correta)
app.UseAuthentication();
app.UseAuthorization();
await app.UseOcelot(); // Inicia o motor de roteamento do Ocelot

app.Run();

// Classes auxiliares para desserializar a configuração do ocelot.json
public class OcelotAuthKey
{
    public string ProviderKey { get; set; } = null!;
    public JwtConfig Config { get; set; } = null!;
}

public class JwtConfig
{
    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
}