using Common.Events;
using MassTransit;

namespace e_commerce.Basket.EventHandlers
{
    public class ProductPriceChangeEventHandler(BasketService service) : IConsumer<ProductPriceUpdates>
    {
        public async Task Consume(ConsumeContext<ProductPriceUpdates> context)
        {
            await service.UpdateBasketItemProductPrices(context.Message.ProductId, context.Message.Price);
        }
    }
}
