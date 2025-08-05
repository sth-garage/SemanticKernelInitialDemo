using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToAudio;
using OpenAI.Images;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001
namespace SemanticKernelWebClient.SK
{
    public class SKQuickTests
    {
        private string _model = "";
        private string _apiKey = "";

        public SKQuickTests(string model, string apiKey)
        {
            _model = model;
            _apiKey = apiKey;
        }

        public async Task RunTests(SKQuickTestOptions options)
        {
            var quickTestPath = @"C:\temp\";
            var imageFilePrefix = "test_image_";
            var audioFilePrefix = "test_audio_";
            var ragFilePath = @"C:\temp\Test.pdf";



            SKQuickTests skQuickTests = new SKQuickTests(_model, _apiKey);
            if (options.ShouldTestImage)
            {
                await skQuickTests.TestImage("Draw a boat", quickTestPath, imageFilePrefix);
            }

            if (options.ShouldTestTextToAudio)
            {
                var textToAudioServe = options.Kernel.GetRequiredService<ITextToAudioService>();

                await skQuickTests.TestAudio("Hello There", quickTestPath, audioFilePrefix, textToAudioServe);
            }

            if (options.ShouldTestLocalRAG)
            {
                var chatCompletionService = options.Kernel.GetRequiredService<IChatCompletionService>();
                await skQuickTests.TestLocalRAG(ragFilePath, chatCompletionService);

            }

        }

        public async Task TestImage(string imageText, string filePath, string fileNamePrefix, int loopCount = 0)
        {
            loopCount = loopCount < 1 ? 1 : loopCount;

            for (int i = 0; i < loopCount; i++)
            {
                // Create the OpenAI ImageClient
                ImageClient client = new("gpt-image-1", _apiKey);

                // Generate the image
                GeneratedImage generatedImage = await client.GenerateImageAsync(imageText,
                    new ImageGenerationOptions
                    {
                        //Size = GeneratedImageSize.W1024xH1024
                    });
                var bytes = generatedImage.ImageBytes;
                var byteArr = bytes.ToArray();
                var safePath = this.GetSafePath(filePath, fileNamePrefix, i);
                File.WriteAllBytes(safePath, byteArr);
            }
        }

        public async Task TestAudio(string audioText, string filePath, string fileName, ITextToAudioService textToAudioService, int loopCount = 0)
        {
            loopCount = loopCount < 1 ? 1 : loopCount;

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

                var safeFilePath = GetSafePath(filePath, fileName, i);

                // Convert text to audio
                AudioContent audioContent = await textToAudioService.GetAudioContentAsync(audioText, executionSettings);

                if (audioContent.Data.HasValue)
                {
                    File.WriteAllBytes(safeFilePath, audioContent.Data.Value.ToArray());
                }
            }

        }

        public async Task TestLocalRAG(string inputFilePath, IChatCompletionService chatCompletionService)
        {
            var chatHistory = new ChatHistory("You are a friendly assistant.");

            var ragQuestion = "";
            for (int i = 0; i < 3; i++)
            {
                var fileBytes = File.ReadAllBytes(inputFilePath);

                ragQuestion = "What's in this file?";
                var withFile = true;

                if (withFile)
                {
                    chatHistory.AddUserMessage(
                    [
                        new TextContent(ragQuestion),
                        new BinaryContent(fileBytes, "application/pdf")
                    ]);

                    withFile = false;
                }
                else
                {
                    chatHistory.AddUserMessage(
                    [
                        new TextContent(ragQuestion),
                    ]);
                }

                var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
            }
        }

        private string GetSafePath(string filePath, string fileNamePrefix, int i)
        {
            var filePathDash = filePath.EndsWith("\\") ? filePath : filePath + "\\";
            var fileName = String.Format(@"{0}{1}.png", fileNamePrefix, i);
            var fullPath = String.Format("{0}{1}");
            return fullPath;
        }

    }

    public class SKQuickTestOptions
    {
        public Kernel Kernel { get; set; }

        public bool ShouldTestImage { get; set; } = false;

        public bool ShouldTestTextToAudio { get; set; } = false;

        public bool ShouldTestLocalRAG { get; set; } = false;
    }

}

