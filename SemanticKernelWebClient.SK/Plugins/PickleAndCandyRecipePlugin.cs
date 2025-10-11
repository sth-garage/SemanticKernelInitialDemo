using Memory;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SemanticKernelWebClient.Models;
using System.ComponentModel;

namespace SemanticKernelWebClient.SK.SKQuickTesting.PluginTests
{

    public class PickleAndCandyRecipePlugin
    {
        public PickleAndCandyRecipePlugin()
        {
        }

        [KernelFunction("get_recipes_with_pickles_and_candy")]
        [Description("Retrieves recipes that include pickles and candy from a RAG database")]
        public async Task<List<string>> GetRecipesWithPicklesAndCandy(Kernel kernel, string questionOrPrompt)
        {
            var embeddingGenerator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

            var vectorStore = new QdrantVectorStore(
               new QdrantClient("localhost"),
               ownsClient: true,
               new QdrantVectorStoreOptions
               {
                   EmbeddingGenerator = embeddingGenerator
               });


            // Create the common processor that works for any vector store.
            var processor = new VectorProcessor(vectorStore, embeddingGenerator);

            var recipes = await processor.Search(RAGCollections.Recipes.ToString(), () => Guid.NewGuid(), questionOrPrompt);
            return recipes.Select(x => x.Record.Definition).ToList();
        }
    }
}
