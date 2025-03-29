using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Text;
using System.Globalization;
using API.Extensions;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Gabonet.Hubble.Extensions;

var builder = WebApplication.CreateBuilder(args);

// builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
//     .ReadFrom.Configuration(context.Configuration));

Console.WriteLine($"MongoDB ConnectionString: {builder.Configuration["MongoDB:ConnectionString"]}");

// Configuración de cultura peruana para toda la aplicación
var peruvianCulture = new CultureInfo("es-PE");
CultureInfo.DefaultThreadCurrentCulture = peruvianCulture;
CultureInfo.DefaultThreadCurrentUICulture = peruvianCulture;

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", 
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Registrar HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configuración de MongoDB
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Registrar el proveedor de logs de Hubble
builder.Logging.AddHubbleLogging(LogLevel.Information);

// Registrar servicios de autenticación
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INotificationService, FirebaseNotificationService>();

// Configuración de JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? "super_secreto_por_defecto"); 

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidIssuer = jwtSettings["Issuer"] ?? "https://tuapi.com",
        ValidAudience = jwtSettings["Audience"] ?? "https://tucliente.com",
        ClockSkew = TimeSpan.Zero
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Error de autenticación: {context.Exception}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validado correctamente");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine($"Token recibido: {context.Token}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services
//     .AddOpenTelemetry()
//     .ConfigureResource(resource => resource.AddService("SchoolTreasure"))
//     .WithTracing(tracing => {
//         tracing
//             .AddHttpClientInstrumentation()
//             .AddAspNetCoreInstrumentation();

//         tracing.AddOtlpExporter(options => {
//             options.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/traces");
//             options.Protocol = OtlpExportProtocol.HttpProtobuf;
//         });
//     });

var app = builder.Build();

// Ejecutar seeders
await Infrastructure.ServiceCollectionExtensions.SeedDatabaseAsync(app.Services);

// Configuración de Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Crear directorio wwwroot si no existe
var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

// Crear directorio uploads si no existe
var uploadsPath = Path.Combine(wwwrootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Servir archivos estáticos
app.UseStaticFiles();

// Usar CORS
app.UseCors("AllowAll");

// Middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();
app.UseRoleAuthorization(); // Middleware personalizado para verificar roles

// app.UseSerilogRequestLogging();
app.UseHubble();

app.MapControllers();

// Modificar para escuchar en todas las interfaces cuando se ejecuta en Docker
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    app.Run("http://0.0.0.0:5200");
}
else
{
    app.Run("http://192.168.18.16:5200");
}
