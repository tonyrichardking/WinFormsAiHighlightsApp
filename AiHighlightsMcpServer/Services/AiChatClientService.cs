//
// https://medium.com/@mutluozkurt/creating-an-mcp-server-and-client-with-net-a-step-by-step-guide-0c3833dde3c4
// https://learn.microsoft.com/en-gb/dotnet/ai/
//

using Anthropic;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OllamaSharp;
using OllamaSharp.Models;
using System.Text;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

public class AiChatClientService : IAsyncDisposable
{
    public AiChatClientService()
    {
        // initialise the runtime options

        defaultSystemPromptKey = "Sports";

        startupMessage = this.InitialiseApi().Result;
        theChosenModel = GetOllamaModelForName(defaultModelName);
        thinking = false;
        useTools = true;
        showFunctionInvocation = false;
        systemPrompt = AvailableSystemPrompts.First(p => p.Key == defaultSystemPromptKey);
        llmTemperature = null;

        SportsApplicationPrompt = getSportsPrompt();
    }


    #region private members

    private static string defaultModelName = "gpt-oss:latest";             //  Claude, llama3.2:latest deepseek-r1:latest; gemma3:latest; deepseek-r1:latest
    private static string ollamaEndPoint = "http://localhost:11434";

    private static List<McpModel> theOllamaModels = new()
            {
                new McpModel("llama3.2:latest", "Meta", "Llama 3.2 Large Language Model", true, false, true),
                new McpModel("deepseek-r1:latest", "DeepSeek", "Performance approaching that of leading models, such as O3 and Gemini 2.5 Pro.", false, true, true),
                new McpModel("qwen3:latest", "Qwen", "The most powerful vision-language model in the Qwen model family to date.", true, true, true),
                new McpModel("gemma3:latest", "Google DeepMind", "Gemma is a family of lightweight, state-of-the-art open models.", true, false, true),
                new McpModel("gpt-oss:latest", "OpenAI", "Powerful reasoning, agentic tasks, and versatile developer use cases.", true, true, true),
                new McpModel("gpt-3.5-turbo", "OpenAI", "Fast and affordable language model.", true, true, true),
                new McpModel("Claude", "Anthropic", "Cloud-based model.", true, true, false),
            };

    // the OllamaApiClient is used to connect to the Ollama server and is the underlying chat client for the MCP client. 
    private IChatClient? ollamaApiClient;

    // The IChatClient interface allows consumption of language models
    private static IChatClient chatClient = null;

    // ChatOptions gets or sets additional per-request instructions: See https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.chatoptions
    private static ChatOptions chatOptions = new();

    #endregion

    #region public members

    public static string SportsApplicationPrompt { get; set; }

    public static string defaultSystemPromptKey { get; set; }

    public static string startupMessage { get; set; }
    public static McpModel theChosenModel { get; set; }
    public static bool thinking { get; set; }
    public static bool useTools { get; set; }
    public static bool showFunctionInvocation { get; set; }
    public static KeyValuePair<string, string> systemPrompt { get; set; }

    public static float? llmTemperature { get; set; }

    // system messages give the model instructions about formality, technical language, or industry-specific terms.
    // https://learn.microsoft.com/en-us/azure/ai-foundry/openai/concepts/advanced-prompt-engineering
    public static Dictionary<string, string> AvailableSystemPrompts = new Dictionary<string, string>
            {
                { "Helpful", "You are a helpful assistant" },
                { "Trump", "You are an AI assistant with the personality of Donald Trump" },
                { "Sarcastic", "You are a sarcastic assistant" },
                { "Sports", getSportsPrompt() }
            };

    public static List<string> AvailableCommands = new List<string>
            {
                "commands - Show commands",
                "exit - Exit the application",
                "quit - Exit the application",
                "model - Change the Ollama model",
                "think - Toggle Thinking",
                "temp - Set the LLMs temperature",
                "tools - Toggle use tools on/off",
                "function - Toggle function call display on/off",
                "system - Change the system prompt. System messages give the model instructions about formality, technical language, or industry-specific terms.",
                "history - Show the chat history",
            };

    public static IList<McpClientTool>? AvailableTools { get; private set; }

    public static List<string>? AvailableModels { get; private set; }

    // ChatHistory retains all the chat context including User and System prompts, and Assistant responses.
    public List<ChatMessage> ChatHistory = [];

    #endregion

    #region public classes

    public class McpModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Vendor { get; set; }
        public bool SupportsTools { get; set; }
        public bool SupportsThinking { get; set; }
        public object? SupportedThoughtLevels { get; set; }
        public bool IsLocalOllama { get; set; }

        public McpModel(string name, string vendor, string description, bool supportsFunctionInvocation, bool supportsThinking, bool isLocalOllama)
        {
            Name = name;
            Description = description;
            Vendor = vendor;
            SupportsTools = supportsFunctionInvocation;
            SupportsThinking = supportsThinking;
            IsLocalOllama = isLocalOllama;
        }

        public override string ToString()
        {
            return $"{Name} by {Vendor} (supports tools: {SupportsTools}, supports thinking: {(SupportsThinking)})";
        }
    }

    #endregion

    #region public methods

    /// <summary>
    /// Main prompt loop method. Takes an API call as a string, sends it to the model, and returns the response as a string. 
    /// The API call can be a user message or a command to change the system prompt, toggle tool usage, or toggle thinking.
    /// </summary>
    /// <param name="apiCall">The API call or user message to send to the model.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the model's response as a string.</returns>
    public async Task<string> RunPrompt(string apiCall)
    {
        // System messages give the model instructions about the assistant. A prompt can have only one system message, and it must be the first message.
        // User messages include prompts from the user and show examples, historical prompts, or contain instructions for the assistant.
        // Assistant messages show example or historical completions, and must contain a response to the preceding user message.
        //
        // A ChatMessage can contain content metadata such as Role, MessageId, as well as the prompt text.
        // The ChatHistory retains all the chat context including User and System prompts, and Assistant responses.

        try
        {
            // build the chat options for each call

            chatOptions = buildChatOptions(AvailableTools, llmTemperature);

            ChatHistory.Add(new ChatMessage(ChatRole.User, apiCall));                // ChatRole can be User, Assistant, System, or Tool

            StringBuilder assistantCompletionSb = new StringBuilder();
            StringBuilder assistantThoughtsSb = new StringBuilder();
            await foreach (ChatResponseUpdate answerToken in chatClient.GetStreamingResponseAsync(ChatHistory, chatOptions))
            {
                if (useTools && showFunctionInvocation)
                {
                    // function call
                    if (answerToken.Role == ChatRole.Assistant && answerToken.Contents.Count == 1 && answerToken.Contents[0] is FunctionCallContent functionCall)
                    {
                        Console.WriteLine($"{renderFunctionCall(functionCall)}");
                        continue;
                    }

                    // function result
                    if (answerToken.Role == ChatRole.Tool && answerToken.Contents.Count == 1 && answerToken.Contents[0] is FunctionResultContent functionResult)
                    {
                        Console.WriteLine($"Function Result: CallId {functionResult.CallId} = {functionResult.Result?.ToString() ?? ""}");
                        continue;
                    }
                }

                // model thoughts
                var thoughts = answerToken.Contents.OfType<TextReasoningContent>();
                if (thoughts.Any())
                {
                    foreach (var thought in thoughts)
                    {
                        assistantThoughtsSb.Append(thought.ToString());
                    }
                }

                // model response
                var completions = answerToken.Contents.OfType<TextContent>();
                if (completions.Any())
                {
                    foreach (var completion in completions)
                    {
                        assistantCompletionSb.Append(completion.Text);
                    }
                }
            }

            StringBuilder resultMessage = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(assistantThoughtsSb.ToString()))
            {
                resultMessage.Append($"\nThoughts:\n{assistantThoughtsSb.ToString()}\n");
            }

            if (!string.IsNullOrWhiteSpace(assistantCompletionSb.ToString()))
            {
                ChatHistory.Add(new ChatMessage(ChatRole.Assistant, assistantCompletionSb.ToString()));
                resultMessage.Append($"{assistantCompletionSb.ToString()}\n");
            }

            return (resultMessage.ToString());
        }
        catch (Exception ex)
        {
            return ($"Error: {ex.Message}");
        }
    }

    public async Task<string> InitialiseApi()
    {
        (string message, ollamaApiClient, AvailableModels, AvailableTools, theChosenModel) = await initialiseApiAsync().ConfigureAwait(false);

        // TODO:- put this somewhere sensible - for now, just add Claude to the list of available models
        AvailableModels.Add("Claude");

        return message;
    }

    public void SetSystemPrompt(KeyValuePair<string, string> prompt)
    {
        (systemPrompt, ChatHistory) = setSystemPrompt(prompt, ChatHistory);
        (chatClient, chatOptions) = buildChatClient(ollamaEndPoint, AvailableTools, theChosenModel, useTools, thinking, llmTemperature);
    }

    public void SetSystemPromptByName(string promptName)
    {
        var prompt = AvailableSystemPrompts.FirstOrDefault(p => p.Key == promptName);
        (systemPrompt, ChatHistory) = setSystemPrompt(prompt, ChatHistory);
        (chatClient, chatOptions) = buildChatClient(ollamaEndPoint, AvailableTools, theChosenModel, useTools, thinking, llmTemperature);
    }

    public string SetFunction()
    {
        showFunctionInvocation = !showFunctionInvocation;
        (chatClient, chatOptions) = buildChatClient(ollamaEndPoint, AvailableTools, theChosenModel, useTools, thinking, llmTemperature);
        return $"Function display set to {showFunctionInvocation}";
    }

    public string SetThinking()
    {
        thinking = setThoughtLevel(theChosenModel, thinking, chatOptions);
        (chatClient, chatOptions) = buildChatClient(ollamaEndPoint, AvailableTools, theChosenModel, useTools, thinking, llmTemperature);
        return theChosenModel.SupportsThinking ? $"Thinking set to {thinking}" : $"Thinking not supported on this model - set to {thinking}.";
    }
    public string SetTemperature(float? chosenTemperature)
    {
        llmTemperature = chosenTemperature;
        (chatClient, chatOptions) = buildChatClient(ollamaEndPoint, AvailableTools, theChosenModel, useTools, thinking, llmTemperature);
        string temperatureDisplay = llmTemperature.HasValue ? llmTemperature.Value.ToString() : "null";
        return $"Temperature set to {temperatureDisplay}.";
    }

    public static string GetOptions()
    {
        return ($"" +
            $"\n- Using {theChosenModel.ToString()}." +
            $"\n- Use thinking is set to {thinking}." +
            $"\n- Temperature is set to {(llmTemperature.HasValue ? llmTemperature.Value.ToString() : "null")}." +
            $"\n- Tool usage is set to {useTools}." +
            $"\n- Show function invocation is set to {showFunctionInvocation}." +
            $"\n- System Prompt is set to '{systemPrompt.Key}'.");
    }

    public void SetTools()
    {
        useTools = !useTools && theChosenModel.SupportsTools;
        (chatClient, chatOptions) = buildChatClient(ollamaEndPoint, AvailableTools, theChosenModel, useTools, thinking, llmTemperature);
    }

    public void SetModel(McpModel chosenModel)
    {
        theChosenModel = chosenModel;
        useTools = theChosenModel.SupportsTools;
        (chatClient, chatOptions) = buildChatClient(ollamaEndPoint, AvailableTools, theChosenModel, useTools, thinking, llmTemperature);
    }

    public void SetModelByName(string modelName)
    {
        McpModel chosenModel = GetOllamaModelForName(modelName);
        SetModel(chosenModel);
        useTools = theChosenModel.SupportsTools;
        (chatClient, chatOptions) = buildChatClient(ollamaEndPoint, AvailableTools, theChosenModel, useTools, thinking, llmTemperature);
    }

    public string GetPromptMessage()
    {
        return ($"" +
            $"\n- Using {theChosenModel.ToString()}." +
            $"\n- Use thinking is set to {thinking}." +
            $"\n- Temperature is set to {(llmTemperature.HasValue ? llmTemperature.Value.ToString() : "null")}." +
            $"\n- Tool usage is set to {useTools}." +
            $"\n- Show function invocation is set to {showFunctionInvocation}." +
            $"\n- System Prompt is set to '{systemPrompt.Key}'.");
    }

    public static McpModel GetOllamaModelForName(string model)
    {
        return theOllamaModels.FirstOrDefault(m => m.Name == model) ?? theOllamaModels[0];
    }

    #endregion

    #region private methods

    private static (IChatClient, ChatOptions) buildChatClient(string ollamaEndPoint, IList<McpClientTool> tools, McpModel clientModel, bool useTools, bool thinking, float? temperature)
    {
        if (clientModel.IsLocalOllama)
        {
            // the OllamaApiClient is used to connect to the Ollama server and is the underlying chat client for the MCP client.
            OllamaApiClient ollamaChatClient = new OllamaApiClient(new OllamaApiClient.Configuration
            {
                Uri = new Uri(ollamaEndPoint),
                Model = clientModel.Name
            });

            chatClient = ollamaChatClient;

            IChatClient resultChatClient = chatClient
                .AsBuilder()
                .UseFunctionInvocation()
                .Build();

            return (resultChatClient, chatOptions);
        }
        else
        {
            // All the Anthropic Claude-specific chat configuration

            AnthropicClient client = new()
            {
                ApiKey = "REDACTED_ANTHROPIC_API_KEY",

                // AuthToken = "your-auth-token-here", // use ApiKey OR AuthToken, not both (mutually exclusive)
                // BaseUrl = "https://your-custom-endpoint.com"
            };

            // this is implemented in buildChatClient
            // Configured using the ANTHROPIC_API_KEY, ANTHROPIC_AUTH_TOKEN and ANTHROPIC_BASE_URL environment variables
            // defaultMaxOutputTokens: 300, claude-opus-4-6", claude-haiku-4-5-20251001
            IChatClient chatClient = client.AsIChatClient(defaultModelId: "claude-haiku-4-5-20251001", defaultMaxOutputTokens: 300) 
                .AsBuilder()
                .UseFunctionInvocation()
                .Build();

            return (chatClient, chatOptions);
        }
    }


    private static string getSportsPrompt()
    {
        string prompt = File.ReadAllText(@"C:\Projects\Experiments_2026\FunWithAiSoccerHighlights\WinFormsAiHighlightsApp\AiHighlightsMcpServer\Prompt Engineering\Claude\System Prompt for Claude v2.txt");     
        //string prompt = Resources.System_Prompt_for_Claude_v2;

        return prompt;
    }

    private static async Task<(string message, OllamaApiClient ollamaApiClient, List<string> availableModels, IList<McpClientTool> availableTools, McpModel theChosenModel)> initialiseApiAsync()
    {
        //
        // Initialisation
        //

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("\nMCP Client Started.");

        OllamaApiClient ollamaApiClient = new OllamaApiClient(ollamaEndPoint, defaultModelName);
        sb.AppendLine($"\nConnected to {ollamaEndPoint} using model {ollamaApiClient.SelectedModel}.");

        //
        // Create the MCP client and configure it to start and connect to your MCP server.
        // Code is common to Ollama and Claude
        // https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/build-mcp-client
        //

        var clientTransport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri("http://localhost:11190/mcp"),
            ConnectionTimeout = TimeSpan.FromSeconds(180),
        });

        McpClient mcpClient;
        try
        {
            mcpClient = await McpClient.CreateAsync(clientTransport).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating MCP client: {ex.Message}");
            return ($"Error creating MCP client: {ex.Message}", ollamaApiClient, new List<string>(), new List<McpClientTool>(), null);
        }

        //
        // List available models and tools
        //

        var models = await ollamaApiClient.ListLocalModelsAsync();
        List<string> availableModels = models.Select(m => m.Name).ToList();

        IList<McpClientTool> availableTools = await mcpClient.ListToolsAsync();

        McpModel theChosenModel = GetOllamaModelForName(defaultModelName);

        (chatClient, chatOptions) =
            buildChatClient(ollamaEndPoint, AvailableTools, theChosenModel, useTools, thinking, llmTemperature);

        return (sb.ToString(), ollamaApiClient, availableModels, availableTools, theChosenModel);
    }

    private static ChatOptions buildChatOptions(IList<McpClientTool> tools, float? temperature)
    {
        ChatOptions chatOptions = new ChatOptions
        {
            Temperature = temperature,
            Tools = [.. tools]
        };

        return (chatOptions);
    }

    private static bool setThoughtLevel(McpModel theChosenModel, bool thinking, ChatOptions chatOptions)
    {
        {
            if (theChosenModel.SupportsThinking)
            {
                thinking = !thinking;
                chatOptions.AddOllamaOption(OllamaOption.Think, thinking);
                //Console.WriteLine($"Thinking set to {thinking}.");
            }
            else
            {
                thinking = false;
                chatOptions.AddOllamaOption(OllamaOption.Think, false);
                //Console.WriteLine($"Thinking not supported on this model - set to {thinking}.");
            }
        }

        return thinking;
    }
    private static (KeyValuePair<string, string>, List<ChatMessage>) setSystemPrompt(KeyValuePair<string, string> systemPrompt, List<ChatMessage> chatHistory)
    {
        // A prompt can have only one system message, and it must be the first message.

        chatHistory.RemoveAll(m => m.Role == ChatRole.System);
        List<ChatMessage> newChatHistory = chatHistory.Prepend(new ChatMessage(ChatRole.System, systemPrompt.Value)).ToList();
        chatHistory = newChatHistory;
        Console.WriteLine($"Added System Prompt: {systemPrompt.Key}");

        return (systemPrompt, newChatHistory);
    }

    private static string renderFunctionCall(FunctionCallContent functionCall)
    {
        var builder = new StringBuilder($"CallId {functionCall.CallId} -> {functionCall.Name ?? "Unknown"}");
        builder.Append('(');

        var separator = "";

        if (functionCall.Arguments is not null)
        {
            foreach (var argument in functionCall.Arguments)
            {
                builder.Append($"{separator}{argument.Key}: {argument.Value}");
                separator = ", ";
            }
        }

        builder.Append(")");
        return (builder.ToString());
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    #endregion
}
