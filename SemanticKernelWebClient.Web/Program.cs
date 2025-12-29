using SemanticKernelWebClient.DAL.Context;
using SemanticKernelWebClient.Shared.Models;
using SemanticKernelWebClient.Shared.Utility;
using SemanticKernelWebClient.SK;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelWebClient.SK;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001


var webBuilder = WebApplication.CreateBuilder(args);

webBuilder.Services.AddControllers();

var configBuilder = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var configValues = UserSecretManager.GetSecrets(configBuilder);


SKBuilder skBuilder = new SKBuilder();
var semanticKernelBuildResult = await skBuilder.BuildSemanticKernel(configValues);

webBuilder.Services.AddDbContext<SemanticKernelWebClientDBContext>(options =>
{
    options.UseSqlServer(configValues.ConnectionStrings.ConnectionString_SemanticKernelWebClient,
        sqlServerOptions => sqlServerOptions.CommandTimeout(600));
});

webBuilder.Services.AddSingleton<IChatCompletionService>(semanticKernelBuildResult.AIServices.ChatCompletionService);
webBuilder.Services.AddSingleton<Kernel>(semanticKernelBuildResult.AIServices.Kernel);
webBuilder.Services.AddSingleton<ConfigurationValues>(configValues);

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
