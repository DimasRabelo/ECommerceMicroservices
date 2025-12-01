// ECommerce.Auth.Api/Program.cs

using Auth.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurações do JWT (leitura do appsettings.json)
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("Jwt"));

// 2. Injeção de Dependência do AuthService
builder.Services.AddScoped<AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers(); // <-- Ele deve conseguir fazer isso agora

app.Run();