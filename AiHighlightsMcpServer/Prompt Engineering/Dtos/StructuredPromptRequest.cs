using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AiHighlightsMcpServer.Prompt_Engineering
{
    public record ExampleStructuredRequests
    {
        public static string firstGoalJson = """
        {
            "structuredPrompt": {
                "metadata": {
                    "createdDateTime": "2026-06-12T11:50:35Z",
                    "guid": "035E5253-E7B2-41A8-81D3-7BA70AC889AA"
                },
                "originalPrompt": "Who scored the first goal?",       
                "curatedPrompt": {
                    "directive": "Solve the following question.  Which player scored the first goal?",
                    "context": "This is in the context of a soccer sports match.",          
                    "outputFormat": "Use a plain text list. Format the output as first name, last name, team, position, period, match time in hours:mins:secs.",
                    "examples": "'Fred Bloggs; Manchester United; Centre Forward; 1st half, 1:15:30",
                    "assistantFeedback": "Do not feed back any reasoning"
                }
            }
        }
    """;
    }

    /// <summary>
    /// CuratedPromptRequest represents a structured prompt sent by the user, containing metadata, user input, 
    /// and system instructions for response formatting. It is designed to facilitate clear communication between 
    /// a user and an AI chat client, allowing for various response formats such as plain text, tables, spreadsheets, 
    /// and JSON for specific use cases like subclipping and spatio-temporal analysis in soccer. 
    /// The structured  format ensures that the system can generate responses that are well-organized and tailored to 
    /// the user's needs while adhering to the specified response formats and purposes.
    /// </summary>
    public sealed record StructuredPromptRequest
    {
        [JsonPropertyName("structuredPrompt")]
        public required StructuredPrompt StructuredPrompt { get; init; }
    }

    public sealed record StructuredPrompt
    {
        [JsonPropertyName("metadata")]
        public required Metadata Metadata { get; init; }   // reuses the Metadata record from your first model

        [JsonPropertyName("originalPrompt")]
        public required string OriginalPrompt { get; init; }

        [JsonPropertyName("curatedPrompt")]
        public required CuratedPrompt CuratedPrompt { get; init; }
    }

    public sealed record CuratedPrompt
    {
        [JsonPropertyName("directive")]
        public required string Directive { get; init; }

        [JsonPropertyName("context")]
        public required string Context { get; init; }

        [JsonPropertyName("outputFormat")]
        public required string OutputFormat { get; init; }

        [JsonPropertyName("examples")]
        public string? Examples { get; init; }

        [JsonPropertyName("assistantFeedback")]
        public string? AssistantFeedback { get; init; }
    }

    public sealed record Metadata
    {
        [JsonPropertyName("createdDateTime")]
        public required DateTimeOffset CreatedDateTime { get; init; }

        [JsonPropertyName("guid")]
        public required Guid Guid { get; init; }
    }


}
