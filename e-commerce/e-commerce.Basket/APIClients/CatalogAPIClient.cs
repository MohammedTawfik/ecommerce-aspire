using e_commerce.Catalog.Models;

namespace e_commerce.Basket.APIClients
{
    public class CatalogAPIClient(HttpClient httpClient)
    {
        public async Task<Product?> GetProductById(int id)
        {
            var response = await httpClient.GetFromJsonAsync<Product>($"/products/{id}");
            return response;
        }
    }
}
