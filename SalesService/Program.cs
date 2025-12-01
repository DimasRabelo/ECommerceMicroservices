using Microsoft.EntityFrameworkCore;
using SalesService.Data; 
using SalesService.Services;
using SalesService.MessageBus;
using Microsoft.OpenApi.Models; // ⬅️ NOVO: Necessário para configurar o Swagger JWT
using Microsoft.AspNetCore.Authentication.JwtBearer; // ⬅️ NOVO: Necessário para autenticação JWT
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÃO DO BANCO DE DADOS (SQLite) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SalesContext>(options =>
    options.UseSqlServer(connectionString)
);

// --- 2. CONFIGURAÇÃO DO HTTP CLIENT (Para comunicação Síncrona com StockService) ---
var stockServiceUrl = builder.Configuration.GetValue<string>("Services:StockServiceUrl");
builder.Services.AddHttpClient("StockApiClient", client =>
{
    client.BaseAddress = new Uri(stockServiceUrl!);
});

// Registra o StockApiClient para injeção
builder.Services.AddScoped<StockApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("StockApiClient");
    return new StockApiClient(httpClient);
});

// --- 3. CONFIGURAÇÃO DO RABBITMQ (Para comunicação Assíncrona) ---
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();


// ----------------------------------------------------------------------
// 4. CONFIGURAÇÃO JWT/AUTENTICAÇÃO (Para proteger os Controllers)
// ----------------------------------------------------------------------
var jwtSecret = builder.Configuration["Jwt:Secret"];
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer, 
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!))
        };
    });
builder.Services.AddAuthorization();


// --- 5. CONFIGURAÇÃO BASE DA API (MVC e Swagger com JWT) ---
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();

// Configuração do Swagger com o cadeado JWT
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT no formato: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline. 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); 

// 6. ADICIONAR MIDDLEWARE DE AUTENTICAÇÃO E AUTORIZAÇÃO (IMPORTANTE NA ORDEM CORRETA)
app.UseAuthentication();
app.UseAuthorization();

// Mapeamento dos Controllers
app.MapControllers(); 

app.Run();