using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using OpenAI;
using OpenAI.VectorStores;
using Qdrant.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.KernelMemory.Constants.CustomContext;

namespace SemanticKernelWebClient.SK.RAG
{
    internal class FinanceInfo
    {
        [VectorStoreKey]
        public Guid Key { get; set; } = Guid.NewGuid();

        [VectorStoreData]
        public string Text { get; set; } = string.Empty;

        // Note that the vector property is typed as a string, and
        // its value is derived from the Text property. The string
        // value will however be converted to a vector on upsert and
        // stored in the database as a vector.
        [VectorStoreVector(1536)]
        public string Embedding => this.Text;
    }

    public class RAGTest
    {
        public async Task Blah(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            // The data model


            // Create an OpenAI embedding generator.
            //var embeddingGenerator = new OpenAIClient("your key")
            //    .GetEmbeddingClient("your chosen model")
            //    .AsIEmbeddingGenerator();

            // Use the embedding generator with the vector store.
            //var vectorStore = new InMemoryVectorStore(new() { EmbeddingGenerator = embeddingGenerator });

            var vectorStore = new QdrantVectorStore(
                new QdrantClient("localhost"),
                ownsClient: true,
                new QdrantVectorStoreOptions
                {
                    EmbeddingGenerator = embeddingGenerator
                });

            var collection = vectorStore.GetCollection<Guid, FinanceInfo>("finances");
            await collection.EnsureCollectionExistsAsync();

            // Create some test data.
            string[] budgetInfo =
            {
                "The budget for 2020 is EUR 100 000",
                "The budget for 2021 is EUR 120 000",
                "The budget for 2022 is EUR 150 000",
                "The budget for 2023 is EUR 200 000",
                "The budget for 2024 is EUR 364 000"
            };

            // Embeddings are generated automatically on upsert.
            var records = budgetInfo.Select((input, index) => new FinanceInfo { Key = Guid.NewGuid(), Text = input });
            //await collection.UpsertAsync(records);

            // Embeddings for the search is automatically generated on search.
            var searchResult = collection.SearchAsync(
                "What is my budget for 2024?",
                top: 1);

            // Output the matching result.
            await foreach (var result in searchResult)
            {
                Console.WriteLine($"Key: {result.Record.Key}, Text: {result.Record.Text}");
            }
        }


        public async Task Blah2(QdrantVectorStore vectorStore)
        {
            // The data model


            // Create an OpenAI embedding generator.
            //var embeddingGenerator = new OpenAIClient("your key")
            //    .GetEmbeddingClient("your chosen model")
            //    .AsIEmbeddingGenerator();

            // Use the embedding generator with the vector store.
            //var vectorStore = new InMemoryVectorStore(new() { EmbeddingGenerator = embeddingGenerator });

            //var vectorStore = new QdrantVectorStore(
            //    new QdrantClient("localhost"),
            //    ownsClient: true,
            //    new QdrantVectorStoreOptions
            //    {
            //        EmbeddingGenerator = embeddingGenerator
            //    });

            var collection = vectorStore.GetCollection<Guid, FinanceInfo>("finances");
            await collection.EnsureCollectionExistsAsync();

            // Create some test data.
            //string[] budgetInfo =
            //{
            //    "The budget for 2020 is EUR 100 000",
            //    "The budget for 2021 is EUR 120 000",
            //    "The budget for 2022 is EUR 150 000",
            //    "The budget for 2023 is EUR 200 000",
            //    "The budget for 2024 is EUR 364 000"
            //};

            // Embeddings are generated automatically on upsert.
            //var records = budgetInfo.Select((input, index) => new FinanceInfo { Key = Guid.NewGuid(), Text = input });
            //await collection.UpsertAsync(records);

            // Embeddings for the search is automatically generated on search.
            var searchResult = collection.SearchAsync(
                "What is my budget for 2024?",
                top: 1);

            // Output the matching result.
            await foreach (var result in searchResult)
            {
                Console.WriteLine($"Key: {result.Record.Key}, Text: {result.Record.Text}");
            }
        }


  
    }


    public class RagTestPlugin
    {
        public RagTestPlugin()
        {
        }

        [KernelFunction("test_rag_plugin_now")]
        public async Task<string> TestRag(Kernel kernel)
        {
            var embeddingGenerator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            //kernel.Services.
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
