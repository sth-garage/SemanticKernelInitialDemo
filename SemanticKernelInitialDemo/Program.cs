// Import packages
using Agents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.Memory;
using OpenAI.Images;
using SemanticKernelInitialDemo.Plugins;
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

ConsoleHelper.SetCurrentFont("Consolas", 22);

var configBuilder = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var apiKey = configBuilder["AI:ApiKey"];
var modelId = configBuilder["AI:Model"];
var apiUrl = configBuilder["AI:ApiUrl"];

var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
    modelId: modelId,
    apiKey: apiKey,
    endpoint: new Uri(apiUrl)
).AddOpenAIChatClient(modelId);

builder.Plugins.AddFromType<LightsPlugin>();
builder.Plugins.AddFromType<ExportPlugin>();
builder.Plugins.AddFromType<TimePlugin>();

// Build the kernel
Kernel kernel = builder.Build();

MixedChat_Agents mixedChat = new MixedChat_Agents();
await mixedChat.ChatWithOpenAIAssistantAgentAndChatCompletionAgent(kernel, apiKey, modelId);

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
