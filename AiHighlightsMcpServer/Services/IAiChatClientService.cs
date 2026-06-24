using AiHighlightsMcpServer.Prompt_Engineering;


/// <summary>
/// Abstracts the LLM orchestration layer so that controllers and other callers
/// can be unit-tested without a live Ollama or Claude connection.
/// Static members of AiChatClientService (AvailableSystemPrompts, AvailableModels,
/// AvailableTools, GetOptions) are not included here because C# interfaces cannot
/// carry static members; callers that need them reference AiChatClientService directly.
/// </summary>
public interface IAiChatClientService
{
    Task<T?> RunWorkInProgressPrompt<T>(string prompt);
    Task<T?> RunPromptUnderTest<T>(string prompt);
    Task<T?> RunTypedPrompt<T>(string prompt);
    Task<string> RunOriginalPrompt(string apiCall);
    void SetModelByName(string modelName);
    void SetSystemPromptByName(string promptName);
}
