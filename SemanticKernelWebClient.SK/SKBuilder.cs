using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.KernelMemory.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Plugins.Core;
using Qdrant.Client;
using SemanticKernelWebClient.Models;
using SemanticKernelWebClient.Plugins;
using SemanticKernelWebClient.SK.SKQuickTesting;
using SemanticKernelWebClient.SK.SKQuickTesting.PluginTests;
using System.Threading.Tasks;
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

namespace SemanticKernelWebClient.SK
{
    public class SKBuilder
    {
        public async Task<SemanticKernelBuilderResult> BuildSemanticKernel(string apiKey, string modelId, string apiUrl, SKQuickTestOptions sKQuickTestOptions = null)
        {
            var skBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: apiKey,
                endpoint: new Uri(apiUrl)

            )
            .AddOpenAIChatClient(modelId)
            .AddOpenAITextToAudio(
                modelId: "gpt-4o-mini-tts",
                apiKey: apiKey
            )
            .AddOpenAIEmbeddingGenerator("text-embedding-3-small", apiKey);

            var servicesTest = skBuilder.Services.ToList();

            skBuilder.Services.AddQdrantVectorStore("localhost", 6333, false, null, new QdrantVectorStoreOptions
            {
               //EmbeddingGenerator = skBuilder.Services[4]
               //EmbeddingGenerator = new E
            });

            // Plugins

            if (false)
            {
                skBuilder.Plugins.AddFromType<LightsPlugin>();
                skBuilder.Plugins.AddFromType<ExportPlugin>();
                skBuilder.Plugins.AddFromType<TimePlugin>();
                

            }

            if (sKQuickTestOptions != null && sKQuickTestOptions.ShouldAddTestRAGPlugin)
            {
                skBuilder.Plugins.AddFromType<RagTestPlugin>();
            }

            // Build the kernel
            Kernel kernel = skBuilder.Build();

            var serviceCollection = new ServiceCollection();
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var embeddingGenerator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var vectorStore = new QdrantVectorStore(
               new QdrantClient("localhost"),
               ownsClient: true,
               new QdrantVectorStoreOptions
               {
                   EmbeddingGenerator = embeddingGenerator
               });

            var isTesting = false;
            if (sKQuickTestOptions != null)
            {
                var skQuickTests = new SKQuickTests(modelId, apiKey);
                await skQuickTests.RunTests(sKQuickTestOptions);
            }
            return new SemanticKernelBuilderResult
            {
                Kernel = kernel,
                ChatCompletionService = chatCompletionService,
                QdrantVectorStore = vectorStore
            };
        }
    }

    
}
