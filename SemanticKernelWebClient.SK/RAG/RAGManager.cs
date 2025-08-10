using Azure.AI.OpenAI;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Identity;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SemanticKernelWebClient.SK.RAG
{
    public class RAGManager
    {
        public async Task InitializeTest(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            // Create an embedding generation service.
            //var embeddingGenerator = new AzureOpenAIClient(new Uri(TestConfiguration.AzureOpenAIEmbeddings.Endpoint), new AzureCliCredential())
            //    .GetEmbeddingClient(TestConfiguration.AzureOpenAIEmbeddings.DeploymentName)
            //    .AsIEmbeddingGenerator(1536);

            // Initiate the docker container and construct the vector store.
            //await qdrantFixture.ManualInitializeAsync();
            var vectorStore = new QdrantVectorStore(new QdrantClient("localhost"), ownsClient: true);

            // Get and create collection if it doesn't exist.
            var collection = vectorStore.GetCollection<ulong, Glossary>("skglossary");
            await collection.EnsureCollectionExistsAsync();

            // Create glossary entries and generate embeddings for them.
            var glossaryEntries = CreateGlossaryEntries().ToList();
            var keys = glossaryEntries.Select(entry => entry.Key).ToList();
            var tasks = glossaryEntries.Select(entry => Task.Run(async () =>
            {
                entry.DefinitionEmbedding = (await embeddingGenerator.GenerateAsync(entry.Definition)).Vector;
            }));
            await Task.WhenAll(tasks);

            // Upsert the glossary entries into the collection and return their keys.
            await collection.UpsertAsync(glossaryEntries);

            // Retrieve one of the upserted records from the collection.
            var upsertedRecord = await collection.GetAsync(keys.First(), new() { IncludeVectors = true });

            // Write upserted keys and one of the upserted records to the console.
            Console.WriteLine($"Upserted record: {JsonSerializer.Serialize(upsertedRecord)}");

        }

        /// <summary>
        /// Setup the qdrant container by pulling the image and running it.
        /// </summary>
        /// <param name="client">The docker client to create the container with.</param>
        /// <returns>The id of the container.</returns>
        public static async Task<string> SetupQdrantContainerAsync(DockerClient client)
        {
            await client.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = "qdrant/qdrant",
                    Tag = "latest",
                },
                null,
                new Progress<JSONMessage>());

            var container = await client.Containers.CreateContainerAsync(new CreateContainerParameters()
            {
                Image = "qdrant/qdrant",
                HostConfig = new HostConfig()
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {"6333", new List<PortBinding> {new() {HostPort = "6333" } }},
                    {"6334", new List<PortBinding> {new() {HostPort = "6334" } }}
                },
                    PublishAllPorts = true
                },
                ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                { "6333", default },
                { "6334", default }
            },
            });

            await client.Containers.StartContainerAsync(
                container.ID,
                new ContainerStartParameters());

            return container.ID;
        }

        private static IEnumerable<Glossary> CreateGlossaryEntries()
        {
            yield return new Glossary
            {
                Key = 1,
                Term = "API",
                Definition = "Application Programming Interface. A set of rules and specifications that allow software components to communicate and exchange data."
            };

            yield return new Glossary
            {
                Key = 2,
                Term = "Connectors",
                Definition = "Connectors allow you to integrate with various services provide AI capabilities, including LLM, AudioToText, TextToAudio, Embedding generation, etc."
            };

            yield return new Glossary
            {
                Key = 3,
                Term = "RAG",
                Definition = "Retrieval Augmented Generation - a term that refers to the process of retrieving additional data to provide as context to an LLM to use when generating a response (completion) to a user’s question (prompt)."
            };
        }
    }

    public class Glossary
    {
        [VectorStoreKey]
        public ulong Key { get; set; }

        [VectorStoreData]
        public string Term { get; set; }

        [VectorStoreData]
        public string Definition { get; set; }

        [VectorStoreVector(1536)]
        public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
    }
}

