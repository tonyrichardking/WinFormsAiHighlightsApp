using System.Text.Json.Serialization;

namespace AiHighlightsMcpServer.Prompt_Engineering
{
    /// <summary>
    /// Payload for the POST aiChat/runTypedPrompt endpoint.
    /// ResultType must be one of the keys returned by GET aiChat/getResultTypes.
    /// </summary>
    public record TypedPromptRequest(
        [property: JsonPropertyName("prompt")]     string Prompt,
        [property: JsonPropertyName("resultType")] string ResultType);
}
