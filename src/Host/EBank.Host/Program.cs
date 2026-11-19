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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddBuildingBlocks();

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

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapControllers();

app.Run();

public partial class Program { }
