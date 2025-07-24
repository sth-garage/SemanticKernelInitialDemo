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
using SemanticKernelInitialDemo.Agents;
using System.ClientModel;
using System.Collections.ObjectModel;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;
using FunctionCallContent = Microsoft.SemanticKernel.FunctionCallContent;
using FunctionResultContent = Microsoft.SemanticKernel.FunctionResultContent;


namespace Agents;

#pragma warning disable OPENAI001
#pragma warning disable SKEXP0110

/// <summary>
/// Demonstrate that two different agent types are able to participate in the same conversation.
/// In this case a <see cref="ChatCompletionAgent"/> and <see cref="OpenAIAssistantAgent"/> participate.
/// </summary>
public class MixedChat_Agents()
{
    private Utility _utility = new Utility();


    public async Task ChatWithOpenAIAssistantAgentAndChatCompletionAgent(Kernel kernel, string key, string model)
    {

        AssistantClient assistantClient = new AssistantClient(key);
        AgentsWithInstructions agentsWithInstructions = new AgentsWithInstructions(kernel, assistantClient, model);

#pragma warning disable OPENAI001

        var chat = await GetMSExampleAgentGroupChat(assistantClient, agentsWithInstructions);
        await BeginChat(chat, "concept: maps made out of egg cartons.");

        var proposal = String.Format(@"""
This essay was written by a student applying to college.  This school is Sam's favorite and they really want to get in.
Create an executive summary from her essay:              
{0}
            """, agentsWithInstructions.SampleCollegeEssayText);

        var essaySummaryChat = await GetEssaySummaryExampleAgentGroupChat(assistantClient, agentsWithInstructions);
        await BeginChat(essaySummaryChat, proposal);

    }

    private async Task<AgentGroupChat>  GetMSExampleAgentGroupChat(AssistantClient assistantClient, 
        AgentsWithInstructions agentsWithInstructions)
    {
        //// Define the assistant
        var assistant = await agentsWithInstructions.GetCopyWriterAssistant();

        // Create the agent
        OpenAIAssistantAgent agentWriter = new OpenAIAssistantAgent(assistant, assistantClient);

        var artDirector = agentsWithInstructions.GetArtDirector();


        // Create a chat for agent interaction.
        //AgentGroupChat chat = null;

        AgentGroupChat chat =
            new(agentWriter, artDirector)
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
                                Agents = [artDirector],
                                // Limit total number of turns
                                MaximumIterations = 10,
                            }
                    }
            };

        return chat;
    }


    private async Task<AgentGroupChat> GetEssaySummaryExampleAgentGroupChat(AssistantClient assistantClient,
        AgentsWithInstructions agentsWithInstructions)
    {
        //// Define the assistant
        var teacherAssistant = await agentsWithInstructions.GetTeacherAssistant();

        // Create the agent
        OpenAIAssistantAgent agentWriter = new OpenAIAssistantAgent(teacherAssistant, assistantClient);

        var friend = agentsWithInstructions.GetFriendAgent();
        var counselor = agentsWithInstructions.GetCounselorAgent();


        // Create a chat for agent interaction.
        //AgentGroupChat chat = null;

        AgentGroupChat chat =
            new(agentWriter, friend, counselor)
            {
                ExecutionSettings =
                    new()
                    {
                        // Here a TerminationStrategy subclass is used that will terminate when
                        // an assistant message contains the term "approve".
                        TerminationStrategy =
                            new ApprovalTerminationStrategy("ApprovedAndDone")
                            {
                                // Only the art-director may approve.
                                Agents = [friend],
                                // Limit total number of turns
                                MaximumIterations = 30,
                            }
                    }
            };

        return chat;
    }

    private async Task BeginChat(AgentGroupChat agentGroupChat, string topic)
    {
        // Invoke chat and display messages.
        ChatMessageContent input = new(AuthorRole.User, topic);
        agentGroupChat.AddChatMessage(input);
        _utility.WriteAgentChatMessage(input);

        await foreach (ChatMessageContent response in agentGroupChat.InvokeAsync())
        {
            _utility.WriteAgentChatMessage(response);
        }

        Console.WriteLine($"\n[IS COMPLETED: {agentGroupChat.IsComplete}]");
    }

#pragma warning disable SKEXP0110
    private sealed class ApprovalTerminationStrategy(string valueToCauseAnExit = "approve") : TerminationStrategy
    {
        // Terminate when the final message contains the term "approve"
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
            => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
    }


    public virtual async Task<ClientResult<Assistant>> CreateAssistantAsync(string model, AssistantCreationOptions options = null, CancellationToken cancellationToken = default)
    {
        ClientResult protocolResult = await CreateAssistantAsync(model, options, cancellationToken).ConfigureAwait(false);
        return ClientResult.FromValue((Assistant)protocolResult, protocolResult.GetRawResponse());
    }

}

