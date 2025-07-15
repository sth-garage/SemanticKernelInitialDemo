// Copyright (c) Microsoft. All rights reserved.

using DocumentFormat.OpenXml.Math;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Chat;
using SemanticKernelInitialDemo;
using System.ClientModel;
using System.Collections.ObjectModel;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;
using FunctionCallContent = Microsoft.SemanticKernel.FunctionCallContent;
using FunctionResultContent = Microsoft.SemanticKernel.FunctionResultContent;


namespace Agents;

#pragma warning disable OPENAI001
/// <summary>
/// Demonstrate that two different agent types are able to participate in the same conversation.
/// In this case a <see cref="ChatCompletionAgent"/> and <see cref="OpenAIAssistantAgent"/> participate.
/// </summary>
public class MixedChat_Agents()
{
    private const string ReviewerName = "ArtDirector";
    private const string ReviewerInstructions =
        """
        You are an art director who has opinions about copywriting born of a love for David Ogilvy.
        The goal is to determine is the given copy is acceptable to print.
        If so, state that it is approved.
        If not, provide insight on how to refine suggested copy without example.
        """;

    private const string CopyWriterName = "CopyWriter";
    private const string CopyWriterInstructions =
        """
        You are a copywriter with ten years of experience and are known for brevity and a dry humor.
        The goal is to refine and decide on the single best copy as an expert in the field.
        Only provide a single proposal per response.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        Consider suggestions when refining an idea.
        """;


    public AssistantClient Client { get; }

    public async Task ChatWithOpenAIAssistantAgentAndChatCompletionAgent(bool useChatClient)
    {
        var model = "gpt-4.1-nano";

        // Define the agents: one of each type
        ChatCompletionAgent agentReviewer =
            new()
            {
                Instructions = ReviewerInstructions,
                Name = ReviewerName,
                Kernel = this.CreateKernelWithChatCompletion(useChatClient, out var chatClient),
            };

        AssistantClient assistantClient = new AssistantClient(PoorMansSecurity.Key);

#pragma warning disable OPENAI001
        // Define the assistant
        Assistant assistant =
            await assistantClient.CreateAssistantAsync(
                model,
                new AssistantCreationOptions
                {
                    Name = CopyWriterName,
                    Instructions = CopyWriterInstructions,
                });

        // Create the agent
        OpenAIAssistantAgent agentWriter = new OpenAIAssistantAgent(assistant, assistantClient);

#pragma warning disable SKEXP0110
        // Create a chat for agent interaction.
        //AgentGroupChat chat = null;
        AgentGroupChat chat =
            new(agentWriter, agentReviewer)
            {
                ExecutionSettings =
                    new()
                    {
                        // Here a TerminationStrategy subclass is used that will terminate when
                        // an assistant message contains the term "approve".
                        TerminationStrategy =
                            new ApprovalTerminationStrategy()
                            {
                                // Only the art-director may approve.
                                Agents = [agentReviewer],
                                // Limit total number of turns
                                MaximumIterations = 10,
                            }
                    }
            };

        // Invoke chat and display messages.
        ChatMessageContent input = new(AuthorRole.User, "concept: maps made out of egg cartons.");
        chat.AddChatMessage(input);
        this.WriteAgentChatMessage(input);

        await foreach (ChatMessageContent response in chat.InvokeAsync())
        {
            this.WriteAgentChatMessage(response);
        }

        Console.WriteLine($"\n[IS COMPLETED: {chat.IsComplete}]");

        chatClient?.Dispose();
    }


    protected AssistantClient AssistantClient { get; }

#pragma warning disable SKEXP0110
    private sealed class ApprovalTerminationStrategy : TerminationStrategy
    {
        // Terminate when the final message contains the term "approve"
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
            => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public virtual async Task<ClientResult<Assistant>> CreateAssistantAsync(string model, AssistantCreationOptions options = null, CancellationToken cancellationToken = default)
    {
        //Argument.AssertNotNullOrEmpty(model, nameof(model));
        //options ??= new();
        //options.Model = model;

        ClientResult protocolResult = await CreateAssistantAsync(model, options, cancellationToken).ConfigureAwait(false);
        return ClientResult.FromValue((Assistant)protocolResult, protocolResult.GetRawResponse());
    }

    protected Kernel CreateKernelWithChatCompletion(bool useChatClient, out IChatClient? chatClient, string? modelName = null)
    {
        var builder = Kernel.CreateBuilder();

        if (useChatClient)
        {
            chatClient = AddChatClientToKernel(builder);
        }
        else
        {
            chatClient = null;
            AddChatCompletionToKernel(builder, modelName);
        }

        return builder.Build();
    }
    

    protected IChatClient AddChatClientToKernel(IKernelBuilder builder)
    {
        //var key = "";
        //var model = "";

#pragma warning disable CA2000 // Dispose objects before losing scope
        IChatClient chatClient;
        //if (this.UseOpenAIConfig)
        //{
        chatClient = new OpenAI.OpenAIClient(PoorMansSecurity.Key)
            .GetChatClient(PoorMansSecurity.Model)
            .AsIChatClient();
        //}
        //else if (!string.IsNullOrEmpty(this.ApiKey))
        //{
        //    chatClient = new AzureOpenAIClient(
        //            endpoint: new Uri(TestConfiguration.AzureOpenAI.Endpoint),
        //            credential: new ApiKeyCredential(TestConfiguration.AzureOpenAI.ApiKey))
        //        .GetChatClient(TestConfiguration.AzureOpenAI.ChatDeploymentName)
        //        .AsIChatClient();
        //}
        //else
        //{
        //    chatClient = new AzureOpenAIClient(
        //            endpoint: new Uri(TestConfiguration.AzureOpenAI.Endpoint),
        //            credential: new AzureCliCredential())
        //        .GetChatClient(TestConfiguration.AzureOpenAI.ChatDeploymentName)
        //        .AsIChatClient();
        //}

        var functionCallingChatClient = chatClient!.AsBuilder().UseKernelFunctionInvocation().Build();
        builder.Services.AddTransient<IChatClient>((sp) => functionCallingChatClient);
        return functionCallingChatClient;
#pragma warning restore CA2000 // Dispose objects before losing scope
    }



    protected Kernel CreateKernelWithChatCompletion(string? modelName = null)
        => this.CreateKernelWithChatCompletion(useChatClient: false, out _, modelName);


    protected void AddChatCompletionToKernel(IKernelBuilder builder, string? modelName = null)
    {
        var key = "";
        var model = "";

        //if (this.UseOpenAIConfig)
        //{
        builder.AddOpenAIChatCompletion(
            model,
            key);
        //}
        //else if (!string.IsNullOrEmpty(this.ApiKey))
        //{
        //    builder.AddAzureOpenAIChatCompletion(
        //        modelName ?? TestConfiguration.AzureOpenAI.ChatDeploymentName,
        //        TestConfiguration.AzureOpenAI.Endpoint,
        //        TestConfiguration.AzureOpenAI.ApiKey);
        //}
        //else
        //{
        //    builder.AddAzureOpenAIChatCompletion(
        //        modelName ?? TestConfiguration.AzureOpenAI.ChatDeploymentName,
        //        TestConfiguration.AzureOpenAI.Endpoint,
        //        new AzureCliCredential());
        //}
    }

#pragma warning disable SKEXP0001
    protected void WriteAgentChatMessage(Microsoft.SemanticKernel.ChatMessageContent message)
    {
        // Include ChatMessageContent.AuthorName in output, if present.
        string authorExpression = message.Role == AuthorRole.User ? string.Empty : $" - {message.AuthorName ?? "*"}";
        // Include TextContent (via ChatMessageContent.Content), if present.
        string contentExpression = string.IsNullOrWhiteSpace(message.Content) ? string.Empty : message.Content;
        bool isCode = message.Metadata?.ContainsKey(OpenAIAssistantAgent.CodeInterpreterMetadataKey) ?? false;
        string codeMarker = isCode ? "\n  [CODE]\n" : " ";
        Console.WriteLine($"\n# {message.Role}{authorExpression}:{codeMarker}{contentExpression}");

        // Provide visibility for inner content (that isn't TextContent).
        foreach (KernelContent item in message.Items)
        {
            if (item is AnnotationContent annotation)
            {
                if (annotation.Kind == AnnotationKind.UrlCitation)
                {
                    Console.WriteLine($"  [{item.GetType().Name}] {annotation.Label}: {annotation.ReferenceId} - {annotation.Title}");
                }
                else
                {
                    Console.WriteLine($"  [{item.GetType().Name}] {annotation.Label}: File #{annotation.ReferenceId}");
                }
            }
            else if (item is FileReferenceContent fileReference)
            {
                Console.WriteLine($"  [{item.GetType().Name}] File #{fileReference.FileId}");
            }
            else if (item is ImageContent image)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {image.Uri?.ToString() ?? image.DataUri ?? $"{image.Data?.Length} bytes"}");
            }
            else if (item is FunctionCallContent functionCall)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {functionCall.Id}");
            }
            else if (item is FunctionResultContent functionResult)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {functionResult.CallId} - {functionResult.Result?.AsJson() ?? "*"}");
            }
        }

        if (message.Metadata?.TryGetValue("Usage", out object? usage) ?? false)
        {
            if (usage is RunStepTokenUsage assistantUsage)
            {
                WriteUsage(assistantUsage.TotalTokenCount, assistantUsage.InputTokenCount, assistantUsage.OutputTokenCount);
            }
            //else if (usage is RunStepCompletionUsage agentUsage)
            //{
            //    WriteUsage(agentUsage.TotalTokens, agentUsage.PromptTokens, agentUsage.CompletionTokens);
            //}
            else if (usage is ChatTokenUsage chatUsage)
            {
                WriteUsage(chatUsage.TotalTokenCount, chatUsage.InputTokenCount, chatUsage.OutputTokenCount);
            }
        }

        void WriteUsage(long totalTokens, long inputTokens, long outputTokens)
        {
            Console.WriteLine($"  [Usage] Tokens: {totalTokens}, Input: {inputTokens}, Output: {outputTokens}");
        }
    }

    protected const string SampleMetadataKey = "sksample";

    protected static readonly ReadOnlyDictionary<string, string> SampleMetadata =
        new(new Dictionary<string, string>
        {
            { SampleMetadataKey, bool.TrueString }
        });
}

