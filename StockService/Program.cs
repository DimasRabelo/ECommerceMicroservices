using Microsoft.EntityFrameworkCore;
using StockService.Data; 
using Microsoft.OpenApi.Models; // ⬅️ NOVO: Necessário para configurar o Swagger JWT
using Microsoft.AspNetCore.Mvc; 

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURAÇÃO DO DBCONTEXT (SQL SERVER ou SQLITE)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Nota: Você está usando UseSqlite, garantindo que o EF Core Sqlite esteja instalado.
builder.Services.AddDbContext<StockContext>(options =>
   options.UseSqlServer(connectionString)
);

// 2. ADICIONAR SERVIÇOS DE CONTROLLERS (MVC)
builder.Services.AddControllers(); 

// REGISTRAR O CONSUMER DO RABBITMQ (IHostedService)
builder.Services.AddHostedService<VendaEfetuadaConsumer>();


// 3. CONFIGURAÇÃO DO SWAGGER/OPENAPI (Documentação e JWT)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 3.1. Definir o esquema de segurança (JWT Bearer)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT no formato: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // 3.2. Aplicar o requisito de segurança (o cadeado 🔒) em todos os endpoints
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

// 4. MAPEAMENTO DOS CONTROLLERS (Essencial para rodar Controllers)
app.MapControllers(); 

app.Run();