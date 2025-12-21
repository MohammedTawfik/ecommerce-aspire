using e_commerce.Basket.Services;

namespace e_commerce.Basket.Endpoints
{
    public static class BasketEndpoints
    {
        public static void MapBasketEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("basket");

            group.MapGet("/{userName}", async (string userName, BasketService service) =>
            {
                var basket = await service.GetUserShoppingCart(userName);
                if (basket is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(basket);
            })
                .WithName("GetUserBasket")
                .Produces(StatusCodes.Status404NotFound)
                .Produces<ShoppingCart>(StatusCodes.Status200OK)
                .RequireAuthorization();

            group.MapPost("/", async (ShoppingCart cart, BasketService service) => 
            {
                await service.UpdateBasket(cart);
                return Results.NoContent();
            })
                .WithName("UpdateBasket")
                .Produces(StatusCodes.Status204NoContent)
                .RequireAuthorization();

            group.MapDelete("/{userName}", async (string userName, BasketService service) =>
            {
                await service.RemoveBasket(userName);
                return Results.NoContent();
            })
                .WithName("DeleteBasket")
                .Produces(StatusCodes.Status204NoContent)
                .RequireAuthorization();
        }
    }
}
