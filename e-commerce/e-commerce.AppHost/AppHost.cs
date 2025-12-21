using System.Runtime.InteropServices.Marshalling;
using Microsoft.Extensions.Hosting;

Console.WriteLine("DOCKER_HOST=" + Environment.GetEnvironmentVariable("DOCKER_HOST"));
Console.WriteLine("DOCKER_CONTEXT=" + Environment.GetEnvironmentVariable("DOCKER_CONTEXT"));

var builder = DistributedApplication.CreateBuilder(args);

var postgress = builder.AddPostgres("db-postgres")
    .WithImagePullPolicy(ImagePullPolicy.Missing)
    .WithPgAdmin()
    .WithDataVolume("e-commerce-database-volume");

var catalogDb = postgress.AddDatabase("catalog-db");

var redis = builder.AddRedis("caching-redis")
    .WithRedisInsight()
    .WithDataVolume("e-commerce-redis-volume");

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume("e-commerce-rabbitmq-volume");


var keyCloack = builder.AddKeycloak("keycloack", 6001)
    .WithDataVolume("ecommerce-keycloak-data")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithExternalHttpEndpoints();


var catalog=builder.AddProject<Projects.e_commerce_Catalog>("e-commerce-catalog")
    .WithReference(catalogDb)
    .WithReference(rabbitMq)
    .WaitFor(catalogDb)
    .WaitFor(rabbitMq);

builder.AddProject<Projects.e_commerce_Basket>("e-commerce-basket")
    .WithReference(redis)
    .WithReference(catalog)
    .WithReference(rabbitMq)
    .WithReference(keyCloack)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(keyCloack);

builder.Build().Run();
