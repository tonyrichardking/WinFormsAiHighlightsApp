using AiHighlightsMcpServer.Prompt_Engineering;

namespace AiHighlightsMcpServer.Services
{
    /// Server-side. Turns a free-text prompt into grounded, match-time events.
    /// Media-agnostic: no mapper, no padding, no FFmpeg — those now live client-side
    /// (eventually Curator-side), because they need the specific media file + its sidecar.
    /// NOTE: "HighlightReel" is now a misnomer — it no longer builds a reel. Consider
    /// renaming to MatchEventService / EventQueryService.
    public class SoccerMatchInfoService
    {
        private readonly AiChatClientService _chat;

        public SoccerMatchInfoService(AiChatClientService chat) => _chat = chat;

        /// Prompt → match-time events. Returns null if nothing matched, so the
        /// controller can distinguish "no results" from an empty-but-valid call.
        public async Task<MatchEventList?> FindEventsAsync(string prompt)
        {
            var events = await _chat.RunWorkInProgressPrompt<MatchEventList>(prompt);
            if (events is null || events.Events.Length == 0)
            {
                return null;
            }

            return events;
        }
    }
}
