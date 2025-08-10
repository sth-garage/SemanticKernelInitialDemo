using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using SemanticKernelWebClient.Plugins;
using SemanticKernelWebClient.SK.RAG;
using System.Threading.Tasks;
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

namespace SemanticKernelWebClient.SK
{
    public class SKBuilder
    {
        public async Task<SemanticKernelBuilderResult> BuildSemanticKernel(string apiKey, string modelId, string apiUrl)
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

            // Plugins

            if (false)
            {
                skBuilder.Plugins.AddFromType<LightsPlugin>();
                skBuilder.Plugins.AddFromType<ExportPlugin>();
                skBuilder.Plugins.AddFromType<TimePlugin>();
            }


            // Build the kernel
            Kernel kernel = skBuilder.Build();


            var serviceCollection = new ServiceCollection();
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var embeddingGenerator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();


            RAGTest test = new RAGTest();
            await test.Blah(embeddingGenerator);


            serviceCollection.AddSingleton<Kernel>();
            serviceCollection.AddSingleton<IChatCompletionService>(chatCompletionService);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            return new SemanticKernelBuilderResult
            {
                Kernel = kernel,
                ChatCompletionService = chatCompletionService,
            };
        }
    }

    public class SemanticKernelBuilderResult
    {
        public IChatCompletionService ChatCompletionService { get; set; }

        public Kernel Kernel { get; set; }
    }
}
