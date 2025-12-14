using Microsoft.AspNetCore.Http.HttpResults;

namespace e_commerce.Catalog.Endpoints
{
    public static class ProductsEndPoints
    {
        public static void MapProductsEndpoints(this IEndpointRouteBuilder routeBuilder)
        {
            var group = routeBuilder.MapGroup("/products");

            group.MapGet("/", async (ProductService service) => { 
                var products = await service.GetAllPRoductsAsync();
                return Results.Ok(products);
            })
                .WithName("GetAllProducts")
                .Produces<List<Product>>(StatusCodes.Status200OK);

            group.MapGet("/{id}", async (int id, ProductService service) => {
                var product = await service.GetProductByIdAsync(id);
                if (product is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(product);
            })
                .WithName("GetProductById")
                .Produces<Product>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/", async (Product product, ProductService service) =>
            {
                await service.CreateProductAsync(product);
                return Results.Created($"/products/product.id", product);
            })
                .WithName("CreateProduct")
                .Produces<Product>(StatusCodes.Status201Created);

            group.MapPut("/{id}", async (int id, Product product, ProductService service) => {
                product.Id = id;
                var result = await service.UpdateProductAsync(product);
                if(result is null)
                {
                    return Results.NotFound();
                }
                return Results.NoContent();
            })
                .WithName("UpdateProduct")
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status204NoContent);

            group.MapDelete("/{id}", async (int id, ProductService service) => {
                var result = await service.DeleteProductAsync(id);
                if(result is null)
                {
                    return Results.NotFound();
                }
                return Results.NoContent();
            })
                .WithName("DeleteProduct")
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status204NoContent); ;

        }
    }
}
