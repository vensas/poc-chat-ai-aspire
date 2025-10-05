using System.ClientModel;
using Microsoft.SemanticKernel;
using OpenAI;

public static class KernelBuilderExtensions
{
    public static IKernelBuilder AddCustomChatCompletion(this IKernelBuilder builder, string model = "mistral", string endpoint = "http://localhost:9999")
    {
        // Ollama exposes an OpenAI compatible API at /v1, so we can use the OpenAIClient
        OpenAIClientOptions options = new OpenAIClientOptions
        {
            Endpoint = new Uri($"{endpoint}/v1")
        };
        OpenAIClient openAIClient = new OpenAIClient(new ApiKeyCredential("none"), options);
        builder.Services.AddSingleton(openAIClient);
        builder.AddOpenAIChatCompletion(model, openAIClient);
        return builder;
    }
}