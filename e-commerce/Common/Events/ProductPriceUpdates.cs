namespace Common.Events
{
    public record ProductPriceUpdates(int ProductId, string ProductName, string ProductDescription, decimal Price, string ImageUrl) : IntegrationEvent;
}
