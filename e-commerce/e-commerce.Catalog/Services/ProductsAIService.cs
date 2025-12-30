using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System.Text;

namespace e_commerce.Catalog.Services
{
    public class ProductsAIService(IOllamaApiClient ollamaApiClient)
    {
        public async Task<string> SupportAsync(string query)
        {
            var systemPrompt = """
                You are a useful assistant.
                you always reply with a short and funny message.
                if you do not know an answer, you say 'i don't know that.'.
                you only answer questions related to outdoor camping products.
                for any other type of questions, explain to the user that you only answer outdoor camping products.
                At the end, Offer one of our products: Hiking Poles-$24, Outdoor Rain Jacket-$12, Outdoor Backpack-$32, Camping Tent-$22
                Do not store memory of the chat conversation.
                """;
            var request = new ChatRequest
            {
                Model = ollamaApiClient.SelectedModel,
                Messages = new List<Message>
            {
                new Message
                {
                    Role = OllamaSharp.Models.Chat.ChatRole.System,
                    Content = systemPrompt,
                },
               new Message
                {
                    Role = OllamaSharp.Models.Chat.ChatRole.User,
                    Content = query,
                },
            }
            };

            var result= new StringBuilder();
            // Use await foreach to iterate through the stream
            await foreach (var response in ollamaApiClient.ChatAsync(request))
            {
                if (response?.Message?.Content != null)
                {
                    Console.Write(response.Message.Content);
                    result.AppendLine(response.Message.Content);
                }
            }
            return result.ToString();

        }
    }
}
