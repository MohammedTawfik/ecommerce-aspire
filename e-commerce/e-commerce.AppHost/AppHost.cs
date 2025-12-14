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


builder.AddProject<Projects.e_commerce_Catalog>("e-commerce-catalog")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

builder.Build().Run();
