using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Plugins.Core;
using SemanticKernelWebClient.Plugins;
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

namespace SemanticKernelWebClient.SK
{
    public class SKBuilder
    {
        public SemanticKernelBuilderResult BuildSemanticKernel(string apiKey, string modelId, string apiUrl)
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
            );

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
