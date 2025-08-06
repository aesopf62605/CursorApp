using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.AspNetCore.Swagger;
using Prometheus;
using InvoiceApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks()
    .ForwardToPrometheus();

// Dependency injection for services and Fixer client
builder.Services.AddHttpClient();
builder.Services.AddSingleton<InvoiceApi.Services.IMetricsService, InvoiceApi.Services.MetricsService>();
builder.Services.AddScoped<InvoiceApi.Clients.IExchangeRateClient, InvoiceApi.Clients.FixerExchangeRateClient>();
builder.Services.AddScoped<InvoiceApi.Services.InvoiceCalculatorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Add Prometheus metrics endpoint
app.UseMetricServer();
app.UseHttpMetrics();

// Add custom metrics middleware
app.UseMetrics();

app.MapControllers();

// Map health checks
app.MapHealthChecks("/health");

app.Run();
