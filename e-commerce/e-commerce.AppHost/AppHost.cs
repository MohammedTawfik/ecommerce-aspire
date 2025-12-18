using System.Runtime.InteropServices.Marshalling;


Console.WriteLine("DOCKER_HOST=" + Environment.GetEnvironmentVariable("DOCKER_HOST"));
Console.WriteLine("DOCKER_CONTEXT=" + Environment.GetEnvironmentVariable("DOCKER_CONTEXT"));

var builder = DistributedApplication.CreateBuilder(args);

var postgress = builder.AddPostgres("db-postgres")
    .WithImagePullPolicy(ImagePullPolicy.Missing)
    .WithPgAdmin()
    .WithDataVolume("e-commerce-database-volume")
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDb = postgress.AddDatabase("catalog-db");

var redis = builder.AddRedis("caching-redis")
    .WithRedisInsight()
    .WithDataVolume("e-commerce-redis-volume")
    .WithLifetime(ContainerLifetime.Persistent);

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume("e-commerce-rabbitmq-volume")
    .WithLifetime(ContainerLifetime.Persistent);


var catalog=builder.AddProject<Projects.e_commerce_Catalog>("e-commerce-catalog")
    .WithReference(catalogDb)
    .WithReference(rabbitMq)
    .WaitFor(catalogDb)
    .WaitFor(rabbitMq);

builder.AddProject<Projects.e_commerce_Basket>("e-commerce-basket")
    .WithReference(redis)
    .WithReference(catalog)
    .WithReference(rabbitMq)
    .WaitFor(redis)
    .WaitFor(rabbitMq);

builder.Build().Run();
