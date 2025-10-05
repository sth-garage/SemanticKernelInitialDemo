using Memory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Plugins.Core;
using SemanticKernelInitialDemo.DAL;
using SemanticKernelWebClient.Models;
using SemanticKernelWebClient.SK.SKQuickTesting;
using SemanticKernelWebClient.SK.SKQuickTesting.PluginTests;
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

namespace SemanticKernelWebClient.SK
{
    public class SKBuilder
    {
        public async Task<SemanticKernelBuilderResult> BuildSemanticKernelAsync(ConfigurationValues configValues, SKQuickTestOptions sKQuickTestOptions = null)
        {
            var modelId = configValues.OpenAISettings.OpenAI_Model;
            var apiKey = configValues.OpenAISettings.OpenAI_ApiKey;
            var apiUrl = configValues.OpenAISettings.OpenAI_ApiUrl;

            var skBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: apiKey,
                endpoint: new Uri(apiUrl)

            )
            .AddOpenAIChatClient(modelId)
            .AddOpenAITextToAudio(
                modelId: configValues.OpenAISettings.OpenAI_TextToAudioModel,
                apiKey: apiKey
            )
            .AddOpenAIEmbeddingGenerator(configValues.OpenAISettings.OpenAI_EmbeddingModel, apiKey);

            skBuilder.Services.AddSingleton<ConfigurationValues>(configValues);

            skBuilder.Services.AddQdrantVectorStore("localhost", 6333, false, null, new QdrantVectorStoreOptions
            {
            });

            skBuilder.Services.AddDbContext<CookingContext>();

            skBuilder.Services.AddTransient<VectorProcessor>();

            // Plugins
            var usePlugins = true;
            if (usePlugins)
            {
                skBuilder.Plugins.AddFromType<TimePlugin>();
                skBuilder.Plugins.AddFromType<PickleAndCandyRecipePlugin>();
            }

            // Build the kernel
            Kernel kernel = skBuilder.Build();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var embeddingGenerator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var vectorStoreFactory = new VectorStoreFactory(embeddingGenerator);

            var vectorStore = vectorStoreFactory.VectorStore;

            if (sKQuickTestOptions != null)
            {
                var skQuickTests = new SKQuickTests(configValues);
                sKQuickTestOptions.Kernel = kernel;
                await skQuickTests.RunTests(sKQuickTestOptions, chatCompletionService, configValues);
            }
            return new SemanticKernelBuilderResult
            {
                AIServices = new AIServices
                {
                    ChatCompletionService = chatCompletionService,
                    Kernel = kernel,
                    EmbeddingGenerator = embeddingGenerator,
                    QdrantVectorStore = vectorStore
                },
            };
        }

    }
}
