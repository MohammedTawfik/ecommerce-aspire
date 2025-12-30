using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.AddNpgsqlDbContext<ProductsDBContext>(connectionName: "catalog-db");
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductsAIService>();
builder.Services.AddMassTransitWithAssemblies(Assembly.GetExecutingAssembly());
builder.AddOllamaApiClient("ollama-llama3-2")
    .AddChatClient(otel => otel.EnableSensitiveData = true);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();




var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseMigration();
app.MapProductsEndpoints();
app.Run();