using AiHighlightsMcpServer.Prompt_Engineering;

namespace AiHighlightsMcpServer.Services
{
    public interface ISoccerMatchInfoService
    {
        Task<MatchEvent?> FindEventAsync(string prompt);
        Task<MatchEventList?> FindEventsAsync(string prompt);
        Task<PlayerAppearance?> FindPlayerAsync(string prompt);
        Task<PlayerList?> FindPlayersAsync(string prompt);
    }

    /// Server-side. Turns a free-text prompt into grounded, match-time events and players.
    /// Media-agnostic: no mapper, no padding, no FFmpeg — those live client-side.
    public class SoccerMatchInfoService : ISoccerMatchInfoService
    {
        private readonly IAiChatClientService _chat;

        public SoccerMatchInfoService(IAiChatClientService chat) => _chat = chat;

        public async Task<MatchEvent?> FindEventAsync(string prompt)
            => await _chat.RunWorkInProgressPrompt<MatchEvent>(prompt);

        /// Returns null if nothing matched so the controller can distinguish "no results"
        /// from an empty-but-valid response.
        public async Task<MatchEventList?> FindEventsAsync(string prompt)
        {
            var result = await _chat.RunWorkInProgressPrompt<MatchEventList>(prompt);
            return result is null || result.Events.Length == 0 ? null : result;
        }

        public async Task<PlayerAppearance?> FindPlayerAsync(string prompt)
            => await _chat.RunWorkInProgressPrompt<PlayerAppearance>(prompt);

        public async Task<PlayerList?> FindPlayersAsync(string prompt)
        {
            var result = await _chat.RunWorkInProgressPrompt<PlayerList>(prompt);
            return result is null || result.Players.Length == 0 ? null : result;
        }
    }
}
