using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();  //Native OpenAPI generation

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); //Serves /openapi/v1.json
    app.MapScalarApiReference(); //Modern UI
}

app.MapGet("/health", () => Results.Ok(
    new
    {
        Service = "AuthService",
        Status = "Healthy"
    }));

app.MapControllers();
app.Run();