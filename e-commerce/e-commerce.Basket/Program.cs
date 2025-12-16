using e_commerce.Basket.APIClients;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRedisDistributedCache(connectionName: "caching-redis");
builder.Services.AddScoped<BasketService>();

builder.Services.AddHttpClient<CatalogAPIClient>(client => 
{
    client.BaseAddress = new Uri("http://e-commerce-catalog");
});

builder.Services.ConfigureHttpClientDefaults(options =>
{
    options.ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapBasketEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
