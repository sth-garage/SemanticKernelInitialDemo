using Memory;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using SemanticKernelWebClient.Models;
using System.Collections;
using TextContent = Microsoft.SemanticKernel.TextContent;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001
namespace SemanticKernelWebClient.SK.RAG
{
    public class RagUploadManager
    {
        private ConfigurationValues _userSecrets = new ConfigurationValues();
        private IChatCompletionService _chatCompletionService;
        private Kernel _kernel;


        public RagUploadManager(ConfigurationValues userSecrets, IChatCompletionService chatCompletionService, Kernel kernel)
        {
            _userSecrets = userSecrets;
            _chatCompletionService = chatCompletionService;
            _kernel = kernel;
        }

        public async Task<RAGUploadEntry> GetUploadEntryFromFilePathWithContentAsync(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);
            var result = new RAGUploadEntry
            {
                Category = TestCollectionCategories.TestCategory.ToString(),
                Collection = RAGCollections.Recipes.ToString(),
                Terms = new List<string> { "" },
                TextContent = content
            };
            return result;
        }

        public async Task ProcessFileAsync(VectorProcessor processor, string collectionName, string category, List<string> terms, string mimeType, string content)
        {
            await processor.IngestDataAsync(collectionName, () => Guid.NewGuid(), category, terms.First(), content);
        }


        #region Only for Testing


        public static RAGUploadEntry TestGetUploadEntry()
        {
            RAGUploadEntry result = new RAGUploadEntry();
            var path = @"C:\Temp\Upload";
            var fileName = "test.txt";
            var query = "";
            List<string> terms = new List<string>();
            var category = "Test";
            var collection = RAGCollections.TestCollection;

            result = new RAGUploadEntry
            {
                Category = TestCollectionCategories.TestCategory.ToString(),
                Collection = RAGCollections.TestCollection.ToString(),
                Terms = terms
            };


            return result;
        }


        public async Task TestUploadAsync(ConfigurationValues configValues, Kernel kernel, IChatCompletionService chatCompletionService, RAGUploadEntry uploadEntry)
        {

            var embeddingGenerator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var vectorStoreFactory = new VectorStoreFactory(embeddingGenerator);

            var vectorStore = vectorStoreFactory.VectorStore;


            RagUploadManager ragUploadManager = new RagUploadManager(configValues, chatCompletionService, _kernel);

            var processor = new VectorProcessor(vectorStore, embeddingGenerator);
            await processor.IngestDataAsync(uploadEntry.Collection, () => Guid.NewGuid(), uploadEntry.Category, uploadEntry.Terms.FirstOrDefault() ?? "", uploadEntry.TextContent);
        }

        #endregion


    }
}
