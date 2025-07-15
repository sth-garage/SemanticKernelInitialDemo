// Import packages
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.KernelMemory;
using SemanticKernelInitialDemo.Plugins;
using Microsoft.SemanticKernel.Plugins.Core;
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

// Populate values from your OpenAI deployment
var modelId = "qwen/qwen3-1.7b";
var apiUrl = @"http://127.0.0.1:1234/v1/";

ConsoleHelper.SetCurrentFont("Consolas", 22);


var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
modelId: modelId,
apiKey: modelId,
endpoint: new Uri(apiUrl));

builder.Plugins.AddFromType<LightsPlugin>();
builder.Plugins.AddFromType<ExportPlugin>();
builder.Plugins.AddFromType<TimePlugin>();


// Build the kernel
Kernel kernel = builder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
};

// Create a history store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);
