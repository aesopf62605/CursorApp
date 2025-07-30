using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency injection for services and Fixer client
builder.Services.AddHttpClient();
builder.Services.AddScoped<InvoiceApi.Clients.IExchangeRateClient, InvoiceApi.Clients.FixerExchangeRateClient>();
builder.Services.AddScoped<InvoiceApi.Services.InvoiceCalculatorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.MapControllers();

app.Run();
