using System.Runtime.InteropServices.Marshalling;
using Microsoft.Extensions.Hosting;

Console.WriteLine("DOCKER_HOST=" + Environment.GetEnvironmentVariable("DOCKER_HOST"));
Console.WriteLine("DOCKER_CONTEXT=" + Environment.GetEnvironmentVariable("DOCKER_CONTEXT"));

var builder = DistributedApplication.CreateBuilder(args);

var postgress = builder.AddPostgres("db-postgres")
    .WithImagePullPolicy(ImagePullPolicy.Missing)
    .WithPgAdmin();
    //.WithDataVolume("e-commerce-database-volume");



var catalogDb = postgress.AddDatabase("catalog-db");

var redis = builder.AddRedis("caching-redis")
    .WithRedisInsight();
    //.WithDataVolume("e-commerce-redis-volume");

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();
//.WithDataVolume("e-commerce-rabbitmq-volume");


var keyCloack = builder.AddKeycloak("keycloack")
    //.WithDataVolume("ecommerce-keycloak-data")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false");

var ollama = builder.AddOllama("ollama", 11434)
    .WithOpenWebUI();

var llamaModel = ollama.AddModel("llama3.2");

if (builder.ExecutionContext.IsRunMode)
{
    //Data volumes donot work on Azure Containers App so only add when running local development environment
    postgress.WithDataVolume("e-commerce-database-volume");
    redis.WithDataVolume("e-commerce-redis-volume");
    rabbitMq.WithDataVolume("e-commerce-rabbitmq-volume");
    keyCloack.WithDataVolume("ecommerce-keycloak-data");
    ollama.WithDataVolume("e-commerce-ollama-volume");
}


var catalog=builder.AddProject<Projects.e_commerce_Catalog>("e-commerce-catalog")
    .WithReference(catalogDb)
    .WithReference(rabbitMq)
    .WithReference(llamaModel)
    .WaitFor(catalogDb)
    .WaitFor(rabbitMq)
    .WaitFor(llamaModel);

var basket = builder.AddProject<Projects.e_commerce_Basket>("e-commerce-basket")
    .WithReference(redis)
    .WithReference(catalog)
    .WithReference(rabbitMq)
    .WithReference(keyCloack)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(keyCloack);

builder.AddProject<Projects.e_commerce_WebApp>("e-commerce-webapp")
    .WithExternalHttpEndpoints()
    .WithReference(catalog)
    .WithReference(basket)
    .WithReference(redis)
    .WaitFor(catalog)
    .WaitFor(basket);

builder.Build().Run();
