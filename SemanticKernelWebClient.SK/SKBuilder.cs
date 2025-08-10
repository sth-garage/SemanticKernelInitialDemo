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


            skBuilder.Plugins.AddFromType<RagTestPlugin>();

            // Build the kernel
            Kernel kernel = skBuilder.Build();


            var serviceCollection = new ServiceCollection();
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var embeddingGenerator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            //kernel.Services.
            var vectorStore = new QdrantVectorStore(
               new QdrantClient("localhost"),
               ownsClient: true,
               new QdrantVectorStoreOptions
               {
                   EmbeddingGenerator = embeddingGenerator
               });


            RAGTest test = new RAGTest();
            await test.Blah2(vectorStore);

            //serviceCollection.AddSingleton<QdrantVectorStore>(vectorStore);
            //serviceCollection.AddSingleton<Kernel>();
            //serviceCollection.AddSingleton<IChatCompletionService>(chatCompletionService);
            //var serviceProvider = serviceCollection.BuildServiceProvider();

            return new SemanticKernelBuilderResult
            {
                Kernel = kernel,
                ChatCompletionService = chatCompletionService,
                QdrantVectorStore = vectorStore
            };
        }
    }

    public class SemanticKernelBuilderResult
    {
        public IChatCompletionService ChatCompletionService { get; set; }

        public QdrantVectorStore QdrantVectorStore { get; set; }

        public Kernel Kernel { get; set; }
    }
}
