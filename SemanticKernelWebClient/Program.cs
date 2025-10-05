using Agents;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Connections;
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
using SemanticKernelInitialDemo.DAL;
using SemanticKernelWebClient.Models;
using SemanticKernelWebClient.SK;
using System.IO;
using System.Text;
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001


var webBuilder = WebApplication.CreateBuilder(args);

webBuilder.Services.AddControllers();

var configBuilder = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var userSecrets = UserSecretManager.GetSecrets(configBuilder);

// dotnet user-secrets set "MySecretKey" "MySecretValue"

var apiKey = userSecrets.OpenAISettings.OpenAI_ApiKey;
var modelId = userSecrets.OpenAISettings.OpenAI_Model;
var embeddingModel = userSecrets.OpenAISettings.OpenAI_EmbeddingModel;

SKBuilder skBuilder = new SKBuilder();
var semanticKernelBuildResult = await skBuilder.BuildSemanticKernelAsync(userSecrets, new SKQuickTestOptions
{

});

webBuilder.Services.AddSingleton<QdrantVectorStore>(semanticKernelBuildResult.AIServices.QdrantVectorStore);
webBuilder.Services.AddSingleton<IChatCompletionService>(semanticKernelBuildResult.AIServices.ChatCompletionService);
webBuilder.Services.AddSingleton<Kernel>(semanticKernelBuildResult.AIServices.Kernel);
webBuilder.Services.AddSingleton<ConfigurationValues>(userSecrets);


webBuilder.Services.AddQdrantVectorStore("localhost", 6333, false, null, new QdrantVectorStoreOptions
{
    EmbeddingGenerator = semanticKernelBuildResult.AIServices.EmbeddingGenerator
});


webBuilder.Services.AddSingleton<IChatCompletionService>(semanticKernelBuildResult.AIServices.ChatCompletionService);
webBuilder.Services.AddSingleton<Kernel>(semanticKernelBuildResult.AIServices.Kernel);
webBuilder.Services.AddSingleton<ModelAndKey>(new ModelAndKey { Key = apiKey, ModelId = modelId });
webBuilder.Services.AddDbContext<CookingContext>();


// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var app = webBuilder.Build();

// <snippet_UseWebSockets>
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(20)
};

app.UseWebSockets(webSocketOptions);
// </snippet_UseWebSockets>

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
