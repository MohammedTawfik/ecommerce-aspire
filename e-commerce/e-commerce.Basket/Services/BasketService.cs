using e_commerce.Basket.APIClients;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace e_commerce.Basket.Services
{
    public class BasketService(IDistributedCache cache,CatalogAPIClient catalogClient)
    {
        public async Task<ShoppingCart?> GetUserShoppingCart(string userName)
        {
            var basket = await cache.GetStringAsync(userName);
            return string.IsNullOrEmpty(basket) ? null : JsonSerializer.Deserialize<ShoppingCart>(basket);
        }

        public async Task UpdateBasket(ShoppingCart basket)
        {
            foreach (var item in basket.Items)
            {
                var product = await catalogClient.GetProductById(item.ProductId);
                if (product is null) continue;
                item.Price = product.Price;
                item.ProductName = product.Name;
            }
            await cache.SetStringAsync(basket.Username, JsonSerializer.Serialize(basket));
        }

        public async Task RemoveBasket(string userName)
        {
            await cache.RemoveAsync(userName);
        }

        public async Task UpdateBasketItemProductPrices(int productId, decimal price)
        {
            var basket = await GetUserShoppingCart("dummyuser");

            var item = basket!.Items.FirstOrDefault(x => x.ProductId == productId);
            if (item is not null)
            {
                item.Price = price;
                await cache.SetStringAsync(basket.Username, JsonSerializer.Serialize(basket));
            }
        }
    }
}
