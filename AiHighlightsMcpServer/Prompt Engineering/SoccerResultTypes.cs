using System.Text.Json.Serialization;

namespace AiHighlightsMcpServer.Prompt_Engineering
{
    /// <summary>
    /// A player who scored a goal: name, team, and the time they scored.
    /// </summary>
    public record GoalScorer(string PlayerName, string Team, int Period, int TimeMin, int TimeSec);

    /// <summary>
    /// A list of all goals scored in the match.
    /// </summary>
    public record GoalList(GoalScorer[] Goals);

    /// <summary>
    /// A single match event: the type of event, the player involved, and when it occurred.
    /// </summary>
    public record MatchEvent(string EventType, string PlayerName, string Team, int Period, int TimeMin, int TimeSec);

    /// <summary>
    /// A list of match events matching the user's query.
    /// </summary>
    public record MatchEventList(MatchEvent[] Events);

    /// <summary>
    /// A highlight segment with a label and in/out timecodes expressed as Opta feed time
    /// (period + minutes + seconds). The video pipeline converts these to media timecodes.
    /// </summary>
    public record HighlightSegment(
        string Label,
        int InPeriod,  int InTimeMin,  int InTimeSec,
        int OutPeriod, int OutTimeMin, int OutTimeSec);

    /// <summary>
    /// A list of highlight segments that together form a highlights reel.
    /// </summary>
    public record HighlightSegmentList(HighlightSegment[] Segments);

    /// <summary>
    /// A player who appeared in the match: name, team, position, and shirt number.
    /// </summary>
    public record PlayerAppearance(string PlayerName, string Team, string Position, int ShirtNumber);

    /// <summary>
    /// A list of players matching the user's query (e.g. all starters, all substitutes).
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
            ["GoalScorer"]           = "A single goal scorer: player name, team, period, and time.",
            ["GoalList"]             = "All goals scored in the match.",
            ["MatchEvent"]           = "A single match event: type, player, team, period, and time.",
            ["MatchEventList"]       = "A list of match events matching the query.",
            ["HighlightSegment"]     = "A single highlight clip with in/out Opta timecodes.",
            ["HighlightSegmentList"] = "A list of highlight clips that form a highlights reel.",
            ["PlayerAppearance"]     = "A single player: name, team, position, shirt number.",
            ["PlayerList"]           = "A list of players matching the query.",
        };
    }
}
