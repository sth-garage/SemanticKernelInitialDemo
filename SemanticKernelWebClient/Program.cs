using Agents;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.TextToAudio;
using OpenAI.Images;
using SemanticKernelWebClient.Models;
using SemanticKernelWebClient.SK;
using System.IO;
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001


var webBuilder = WebApplication.CreateBuilder(args);

webBuilder.Services.AddControllers();


// AIzaSyDsFlSg9C5UKx2dW0NF82riJySbEVXtstE
// 253652338022

var configBuilder = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var apiKey = configBuilder["AI:ApiKey"];
var modelId = configBuilder["AI:Model"];
var apiUrl = configBuilder["AI:ApiUrl"];


SKBuilder skBuilder = new SKBuilder();
var semanticKernelBuildResult = await skBuilder.BuildSemanticKernel(apiKey, modelId, apiUrl);

webBuilder.Services.AddSingleton<QdrantVectorStore>(semanticKernelBuildResult.QdrantVectorStore);
webBuilder.Services.AddSingleton<IChatCompletionService>(semanticKernelBuildResult.ChatCompletionService);
webBuilder.Services.AddSingleton<Kernel>(semanticKernelBuildResult.Kernel);
webBuilder.Services.AddSingleton<ModelAndKey>(new ModelAndKey { Key = apiKey, ModelId = modelId });





// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var app = webBuilder.Build();

// <snippet_UseWebSockets>
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(webSocketOptions);
// </snippet_UseWebSockets>

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
