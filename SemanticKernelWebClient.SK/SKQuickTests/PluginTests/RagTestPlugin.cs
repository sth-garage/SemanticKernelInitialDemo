using HtmlAgilityPack;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using SemanticKernelWebClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SemanticKernelWebClient.SK.SKQuickTesting.PluginTests
{
    public class RagTestPlugin
    {
        public RagTestPlugin()
        {
        }

        [KernelFunction("test_rag_plugin_now")]
        public async Task<string> TestRag(Kernel kernel)
        {
            var embeddingGenerator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

            //var vectorStore = kernel.GetRequiredService<QdrantVectorStore>();


            HttpClient client = new HttpClient();
            var blah = await client.GetAsync("https://en.wikipedia.org/wiki/Novel");
            var blah2 = await blah.Content.ReadAsStringAsync();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(blah2);

            //var query = $"div[@id='content']";
            HtmlNode node = doc.GetElementbyId("content");
            //string content = node.InnerHtml;



            //HttpWebRequest request = HttpClient.Create(@"https://en.wikipedia.org/wiki/Novel");
            //request.Method = "GET";
            //String test = String.Empty;
            //using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //{
            //    Stream dataStream = response.GetResponseStream();
            //    StreamReader reader = new StreamReader(dataStream);
            //    test = reader.ReadToEnd();
            //    reader.Close();
            //    dataStream.Close();
            //}





            var vectorStore = new QdrantVectorStore(
               new QdrantClient("localhost"),
               ownsClient: true,
               new QdrantVectorStoreOptions
               {
                   EmbeddingGenerator = embeddingGenerator
               });


            var collection = vectorStore.GetCollection<Guid, FinanceInfo>("finances");
            await collection.EnsureCollectionExistsAsync();

            // Embeddings for the search is automatically generated on search.
            var searchResult = collection.SearchAsync(
                "What is my budget for 2024?",
                top: 1);

            // Output the matching result.
            await foreach (var result in searchResult)
            {
                Console.WriteLine($"Key: {result.Record.Key}, Text: {result.Record.Text}");

                return result.Record.Text;
            }

            return null;

            //return search
            var stop = 1;
        }
    }
}
