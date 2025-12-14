namespace e_commerce.Catalog.Services
{
    public class ProductService(ProductsDBContext productsDBContext)
    {
        public async Task CreateProductAsync(Product product)
        {
            productsDBContext.Products.Add(product);
            await productsDBContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> GetAllPRoductsAsync()
        {
            return await productsDBContext.Products.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            var product = productsDBContext.Products.FirstOrDefault(p => p.Id == id);
            return product;
        }

        public async Task<Product?> UpdateProductAsync(Product product)
        {
            var dbProduct = await productsDBContext.Products.FirstOrDefaultAsync(x => x.Id == product.Id);
            if (dbProduct is null)
            {
                return null;
            }
            if (dbProduct is not null)
            {
                dbProduct.Name = product.Name;
                dbProduct.Description = product.Description;
                dbProduct.Price = product.Price;
                dbProduct.ImageUrl = product.ImageUrl;
            }
            productsDBContext.Products.Update(dbProduct);
            await productsDBContext.SaveChangesAsync();
            return dbProduct;
        }

        public async Task<Product?> DeleteProductAsync(int id)
        {
            var product = await productsDBContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
            {  return null; }
            productsDBContext.Products.Remove(product);
            await productsDBContext.SaveChangesAsync();
            return product;
        }
    }
}
