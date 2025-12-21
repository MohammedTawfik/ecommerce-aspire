

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRedisDistributedCache(connectionName: "caching-redis");
builder.Services.AddScoped<BasketService>();
builder.Services.AddMassTransitWithAssemblies(Assembly.GetExecutingAssembly());

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

builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
                            serviceName: "keycloack", 
                            realm: "eshop", 
                            configureOptions: options => 
                            {
                                options.RequireHttpsMetadata = false;
                                options.Audience = "account";
                                
                                // Add logging for debugging
                                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                                {
                                    OnAuthenticationFailed = context =>
                                    {
                                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                                        return Task.CompletedTask;
                                    },
                                    OnTokenValidated = context =>
                                    {
                                        Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
                                        return Task.CompletedTask;
                                    },
                                    OnChallenge = context =>
                                    {
                                        Console.WriteLine($"OnChallenge error: {context.Error}, {context.ErrorDescription}");
                                        return Task.CompletedTask;
                                    }
                                };
                            });
builder.Services.AddAuthorization();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapBasketEndpoints();

app.Run();
