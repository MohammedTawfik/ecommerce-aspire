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


builder.AddProject<Projects.e_commerce_Catalog>("e-commerce-catalog")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

builder.AddProject<Projects.e_commerce_Basket>("e-commerce-basket")
    .WithReference(redis)
    .WaitFor(redis);

builder.Build().Run();
