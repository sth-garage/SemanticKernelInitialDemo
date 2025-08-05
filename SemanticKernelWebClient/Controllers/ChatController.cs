using Agents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using SemanticKernelWebClient.Models;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

#pragma warning disable OPENAI001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

#pragma warning disable OPENAI001

namespace SemanticKernelWebClient.Controllers;

#region snippet_Controller_Connect
public class ChatController : ControllerBase
{
    IChatCompletionService _chatCompletionService = null;
    Kernel _kernel = null;
    ModelAndKey _modelAndKey = null;

    public ChatController(IChatCompletionService chat, Kernel kernel, ModelAndKey modelAndKey)
    {
        _chatCompletionService = chat;
        _kernel = kernel;
        _modelAndKey = modelAndKey;
    }

    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            //aawait ChatController.Send(webSocket, "Hello there!  My name is Semantigator!  How can I help?");
            await Echo(webSocket, _chatCompletionService, _kernel, _modelAndKey);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    #endregion

    private static async Task Echo(WebSocket webSocket, IChatCompletionService chatCompletionService, Kernel kernel, ModelAndKey modelAndKey)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        // Enable planning
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            
        };

        ChatHistory chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("Your name is Semantigator.  You are a helpful assistant that wants the user to get the job done.  You are friendly, happy, and always willing to help.  You speak with enthusiasm.  Occasionally make a alligator joke or pun.  If the users input ever confuses you or is unclear, ask clarifying questions. Start the chat by introducing yourself and asking if there is anything the user needs help with.  Do not respond until the content is done.  Answer in rich html with DIV as the root node");

        var isAgentChat = false;
        var hasAgentQuestion = false;
        AgentPayload agentPayload = null;
        AgentManager agents = new AgentManager();

        //chatHistory.AddSystemMessage("Greet the user and ask them for their name and what can you help them with today");

        //chatHistory.AddSystemMessage("Your name is Semantigator.  You are a helpful assistant that wants the user to get the job done.  You are friendly, happy, and always willing to help.  You speak with enthusiasm.  Occasionally make an alligator joke or pun.  If the users input ever confuses you or is unclear, ask clarifying questions. Start the chat by introducing yourself and asking if there is anything the user needs help with.  Do not respond until the content is done");


        //await ChatController.Send(webSocket, "Hello there!  My name is Semantigator!  How can I help?");

        while (!receiveResult.CloseStatus.HasValue)
        {
            var bytes = new ArraySegment<byte>(buffer, 0, receiveResult.Count);
            var userMessage = Encoding.UTF8.GetString(bytes);

            if (isAgentChat && !hasAgentQuestion)
            {
                hasAgentQuestion = true;
            }

            try
            {
                agentPayload = JsonConvert.DeserializeObject<AgentPayload>(userMessage);
                isAgentChat = true;
                hasAgentQuestion = false;
            }
            catch(Exception ex)
            {
                var stop = 1;
                isAgentChat = false;
            }

            if (hasAgentQuestion && agentPayload != null)
            {
                await agents.ChatWithOpenAIAssistantAgentAndChatCompletionAgent(kernel, modelAndKey.Key, modelAndKey.ModelId, webSocket, agentPayload, userMessage);
                isAgentChat = false;
                hasAgentQuestion= false;
                agentPayload = null;
            }

            chatHistory.AddUserMessage(userMessage);


            ChatMessageContent content = new ChatMessageContent();
            //chatHistory.AddDeveloperMessage("The result should always be in rich HTML format - the root element must be a DIV.  The author name must be displayed in bold on the top line of each message.  Include visual elements like lists, colors, tables when appropriate to provide clarity");


            var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory, 
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            //chatHistory.AddMessage(result.Role, result.Content ?? string.Empty);

            var resultString = result.AsJson();
            var resultMsgBytes = Encoding.UTF8.GetBytes(resultString);


            await webSocket.SendAsync(
                resultMsgBytes,
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);


            try
            {
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch(Exception ex)
            {
                var stop = 1;
            }
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}
