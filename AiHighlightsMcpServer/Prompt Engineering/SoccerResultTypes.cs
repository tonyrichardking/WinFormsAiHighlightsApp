using AiHighlightsMcpServer.Services;
using System.Text.Json.Serialization;

namespace AiHighlightsMcpServer.Prompt_Engineering
{
    // ---- LLM-facing result types (what RunPrompt<T> may return) ----

    /// <summary>
    /// A single match event in match (feed) time: what happened, who was involved, and when.
    /// </summary>
    public record MatchEvent(string EventType, string PlayerName, string Team, int Period, int TimeMin, int TimeSec);

    /// <summary>
    /// A list of match events matching the user's query — goals, fouls, cards, shots, etc.
    /// </summary>
    public record MatchEventList(MatchEvent[] Events);

    /// <summary>
    /// A player who appeared in the match.
    /// </summary>
    public record PlayerAppearance(string PlayerName, string Team); //, string Position, int ShirtNumber); MA3 feed doesn't provide position or shirt number.

    /// <summary>
    /// A list of players matching the user's query — starters, substitutes, a whole team, etc.
    /// </summary>
    public record PlayerList(PlayerAppearance[] Players);

    // ---------------------------------------------------------------------------
    // Catalog — maps the string names used in TypedPromptRequest.ResultType to
    // human-readable descriptions. Update this whenever a new type is added above.
    // ---------------------------------------------------------------------------

    public static class SoccerResultTypeCatalog
    {
        public static readonly Dictionary<string, string> Descriptions = new()
        {
            ["MatchEventList"]       = "A list of match events matching the query.",
            ["PlayerList"]           = "A list of players matching the query.",
        };
    }
}
