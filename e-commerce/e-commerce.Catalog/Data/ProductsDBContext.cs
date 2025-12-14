namespace e_commerce.Catalog.Data
{
    public class ProductsDBContext : DbContext
    {
        public ProductsDBContext(DbContextOptions<ProductsDBContext> options): base(options) { }

        public DbSet<Product> Products { get; set; }
    }
}
