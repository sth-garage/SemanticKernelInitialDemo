using Memory;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.TextToAudio;
using OpenAI.Images;
using Qdrant.Client;

using SemanticKernelWebClient.Models;
using SemanticKernelWebClient.SK.RAG;
using TextContent = Microsoft.SemanticKernel.TextContent;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001
namespace SemanticKernelWebClient.SK.SKQuickTesting
{
    public class SKQuickTests
    {
        private ConfigurationValues _userSecrets = new ConfigurationValues();

        public SKQuickTests(ConfigurationValues userSecrets)
        {
            _userSecrets = userSecrets;
        }


        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public async Task RunTests(SKQuickTestOptions options, IChatCompletionService chatCompletionService, ConfigurationValues configValues)
        {
            
            var quickTestAudioPath = @"C:\temp\";
            var quickTestImagePath = @"C:\temp\";
            var imageFilePrefix = "test_image_";
            var audioFilePrefix = "test_audio_";
            var ragFilePath = @"C:\Users\sholt\source\repos\UPSDocs\ShipExec\General\About ShipExec™ - ShipExecCustPortal.pdf";

            SKQuickTests skQuickTests = new SKQuickTests(_userSecrets);
            if (options.ShouldTestImage)
            {
                var imageDescription = "Draw a boat";

                await skQuickTests.TestImage(imageDescription, quickTestImagePath, imageFilePrefix, 1);
            }

            if (options.ShouldTestTextToAudio)
            {
                var textToAudioServe = options.Kernel.GetRequiredService<ITextToAudioService>();

                await skQuickTests.TestAudioAsync("Hello There", quickTestAudioPath, audioFilePrefix, textToAudioServe);
            }

            if (options.ShouldTestLocalRAG)
            {
                var embeddingGenerator = options.Kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

                var vectorStore = new QdrantVectorStore(
                   new QdrantClient("localhost"),
                   ownsClient: true,
                   new QdrantVectorStoreOptions
                   {
                       EmbeddingGenerator = embeddingGenerator
                   });

                await skQuickTests.TestLocalRAGAdvancedAsync(ragFilePath, chatCompletionService, vectorStore, embeddingGenerator);


            }

            if (options.ShouldTestRAGSearch)
            {
                var embeddingGenerator = options.Kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

                var vectorStore = new QdrantVectorStore(
                   new QdrantClient("localhost"),
                   ownsClient: true,
                   new QdrantVectorStoreOptions
                   {
                       EmbeddingGenerator = embeddingGenerator
                   });

                await skQuickTests.SearchOnExistingAsync(vectorStore);
            }

            if (options.ShouldTestRAGUploadAndSearch)
            {
                var embeddingGenerator = options.Kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

                await skQuickTests.UploadAndSearchAsync(embeddingGenerator);
            }

            if (options.ShouldTestUPSDemo)
            {
                var embeddingGenerator = options.Kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

                var vectorStore = new QdrantVectorStore(
                   new QdrantClient("localhost"),
                   ownsClient: true,
                   new QdrantVectorStoreOptions
                   {
                       EmbeddingGenerator = embeddingGenerator
                   });

                await skQuickTests.IngestAndSearch(vectorStore, embeddingGenerator);
            }

            if (options.ShouldUploadToRag)
            {
                var entry = RagUploadManager.TestGetUploadEntry();
                await UploadToRag(chatCompletionService, options.Kernel, configValues, entry);
            }

        }

        public async Task UploadToRag(IChatCompletionService chatCompletionService, Kernel kernel, ConfigurationValues configValues, RAGUploadEntry entry)
        {
            RagUploadManager ragUploadManager = new RagUploadManager(configValues, chatCompletionService, kernel);

            await ragUploadManager.TestUploadAsync(configValues, kernel, chatCompletionService, entry);
            var done = 1;
        }


        public async Task TestImage(string imageText, string filePath, string fileNamePrefix, int loopCount = 0)
        {
            List<string> instructions = new List<string>
            {

            };


            foreach (var instruction in instructions)
            {

                loopCount = loopCount < 1 ? 1 : loopCount;

                string dateStr = GetFormattedDateTime();

                // Create the OpenAI ImageClient
                ImageClient client = new(_userSecrets.OpenAISettings.OpenAI_ImageModel,
                    _userSecrets.OpenAISettings.OpenAI_ApiKey);


                for (int i = 0; i < loopCount; i++)
                {
                    // Generate the image
                    GeneratedImage generatedImage = await client.GenerateImageAsync(imageText,
                        new OpenAI.Images.ImageGenerationOptions
                        {
                        });
                    var bytes = generatedImage.ImageBytes;
                    var byteArr = bytes.ToArray();
                    var safePath = GetSafePath(filePath, fileNamePrefix, i, dateStr);
                    File.WriteAllBytes(safePath, byteArr);
                }
            }
        }

        private string GetFormattedDateTime()
        {
            return DateTimeOffset.Now.ToString("yyyyMMdd_HHmmss");
        }

        public async Task TestAudioAsync(string audioText, string filePath, string fileName, ITextToAudioService textToAudioService, int loopCount = 0)
        {
            loopCount = loopCount < 1 ? 1 : loopCount;

            string dateStr = GetFormattedDateTime();

            for (int i = 0; i < loopCount; i++)
            {
                // Set execution settings (optional)
                OpenAITextToAudioExecutionSettings executionSettings = new()
                {
                    Voice = "shimmer", // The voice to use when generating the audio.
                                       // Supported voices are alloy, echo, fable, onyx, nova, and shimmer.
                    ResponseFormat = "mp3", // The format to audio in.
                                            // Supported formats are mp3, opus, aac, and flac.
                    Speed = 1.0f // The speed of the generated audio.
                                 // Select a value from 0.25 to 4.0. 1.0 is the default.
                };

                var safeFilePath = GetSafePath(filePath, fileName, i, dateStr);

                // Convert text to audio
                AudioContent audioContent = await textToAudioService.GetAudioContentAsync(audioText, executionSettings);

                if (audioContent.Data.HasValue)
                {
                    File.WriteAllBytes(safeFilePath, audioContent.Data.Value.ToArray());
                }
            }

        }

        public async Task TestLocalRAGAsync(string inputFilePath, IChatCompletionService chatCompletionService)
        {
            var chatHistory = new ChatHistory("You are a friendly assistant.");

            var ragQuestion = "";
            for (int i = 0; i < 1; i++)
            {
                var fileBytes = File.ReadAllBytes(inputFilePath);

                ragQuestion = "Convert this file to text that will be uploaded to a RAG database.  The output needs to include as much information as possible.  Take your time and make sure that all the main points in the image are covered.  The image will be a PDF describing a product called ShipExec.  Do not include any helping text or questions, output just the results of the summary and nothing more.";
                var withFile = true;

                if (withFile)
                {
                    chatHistory.AddUserMessage([
                        new TextContent(ragQuestion),
                        new BinaryContent(fileBytes, "application/pdf")
                        ]);
                }
                else
                {
                    chatHistory.AddUserMessage(
                    [
                        new TextContent(ragQuestion),
                    ]);
                }


                var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
                var stop = 1;
            }
        }


        public async Task TestLocalRAGAdvancedAsync(string inputFilePath, IChatCompletionService chatCompletionService, QdrantVectorStore vectorStore, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            var chatHistory = new ChatHistory("You are a friendly assistant.");

            var ragQuestion = "";
            for (int i = 0; i < 1; i++)
            {
                var fileBytes = File.ReadAllBytes(inputFilePath);

                ragQuestion = "Convert this file to text that will be uploaded to a RAG database.  The output needs to include as much information as possible.  Take your time and make sure that all the main points in the image are covered.  The image will be a PDF describing a product called ShipExec.  Do not include any helping text or questions, output just the results of the summary and nothing more.";
                var withFile = true;

                if (withFile)
                {
                    chatHistory.AddUserMessage([
                        new TextContent(ragQuestion),
                        new BinaryContent(fileBytes, "application/pdf")
                        ]);
                }
                else
                {
                    chatHistory.AddUserMessage(
                    [
                        new TextContent(ragQuestion),
                    ]);
                }

                var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
                var stop = 1;


                // Create the common processor that works for any vector store.
                var processor = new VectorProcessor(vectorStore, embeddingGenerator);

                var seOut = await processor.Search("test", () => Guid.NewGuid(), "What is this file?");
                var testout = 1;
            }
        }

        private string GetSafePath(string filePath, string fileNamePrefix, int i, string dateString)
        {
            var filePathDash = filePath.EndsWith("\\") ? filePath : filePath + "\\";
            var fileName = string.Format(@"{2}_{0}{1}.png", fileNamePrefix, i, dateString);
            var fullPath = string.Format("{0}{1}", filePathDash, fileName);
            return fullPath;
        }

        public async Task UploadAndSearchAsync(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            // The data model
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
            await collection.UpsertAsync(records);

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


        public async Task SearchOnExistingAsync(QdrantVectorStore vectorStore)
        {
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
            }
        }

        public async Task IngestAndSearch(QdrantVectorStore vectorStore, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            // Create the common processor that works for any vector store.
            var processor = new VectorProcessor(vectorStore, embeddingGenerator);

            // Run the process and pass a key generator function to it, to generate unique record keys.
            // The key generator function is required, since different vector stores may require different key types.
            // E.g. Qdrant supports Guid and ulong keys, but others may support strings only.
            await processor.TestIngestDataAndSearchAsync("skglossaryWithoutDI", () => Guid.NewGuid());
        }

    }
}

