using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace e_commerce.Basket.Services
{
    public class BasketService(IDistributedCache cache)
    {
        public async Task<ShoppingCart?> GetUserShoppingCart(string userName)
        {
            var basket = await cache.GetStringAsync(userName);
            return string.IsNullOrEmpty(basket) ? null : JsonSerializer.Deserialize<ShoppingCart>(basket);
        }

        public async Task UpdateBasket(ShoppingCart basket)
        {
            await cache.SetStringAsync(basket.Username, JsonSerializer.Serialize(basket));
        }

        public async Task RemoveBasket(string userName)
        {
            await cache.RemoveAsync(userName);
        }
    }
}
