using System.Text;
using System.Text.Json;
using EBank.Accounting;
using EBank.Aml;
using EBank.BuildingBlocks;
using EBank.BuildingBlocks.Errors;
using EBank.Cards;
using EBank.Cbs;
using EBank.Crm;
using EBank.Kyc;
using EBank.Loans;
using EBank.Notification;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddBuildingBlocks();

// Configuración JWT (NFR-1: Access token válido 5 minutos, validación estricta)
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret is missing");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Expiración exacta a los 5 minutos
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var payload = new ErrorResponse(new ErrorDetail("UNAUTHORIZED", "Valid authentication token is required."));
                await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                var payload = new ErrorResponse(new ErrorDetail("FORBIDDEN", "Insufficient permissions for this resource."));
                await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            }
        };
    });

builder.Services.AddAuthorization();

// Registro de los 8 módulos
builder.Services.AddKycModule(builder.Configuration);
builder.Services.AddCrmModule(builder.Configuration);
builder.Services.AddCbsModule(builder.Configuration);
builder.Services.AddCardsModule(builder.Configuration);
builder.Services.AddLoansModule(builder.Configuration);
builder.Services.AddAccountingModule(builder.Configuration);
builder.Services.AddAmlModule(builder.Configuration);
builder.Services.AddNotificationModule(builder.Configuration);

var app = builder.Build();

// Middleware global para devolver siempre el contrato de error estándar
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (EBankException ex)
    {
        context.Response.StatusCode = ex.StatusCode;
        context.Response.ContentType = "application/json";
        var payload = new ErrorResponse(new ErrorDetail(ex.Code, ex.Message));
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapControllers();

app.Run();

public partial class Program { }
