using AiHighlightsMcpServer.Prompt_Engineering;
using System;
using System.Collections.Generic;
using System.Text;

namespace AiHighlightsWinFormsUi.Orchestration
{
    public enum ResponseKind { Help, EventResult, StructuredResult, FreeformText, Error }

    public record ClientResponse(ResponseKind Kind, string Text);

    public static class LocalCommands
    {
        public static bool IsHelp(string t) =>
            t.Equals("help", StringComparison.OrdinalIgnoreCase) || t == "?";

        public const string HelpText =
            "Type a question about the match and pick a result type:\n" +
            "  • MatchEvent / MatchEventList — goals, fouls, cards… (drives the highlights reel)\n" +
            "  • PlayerAppearance / PlayerList — squad and roster info\n" +
            "Type 'help' or '?' for this message.";
    }

    // UI-agnostic on purpose: no WinForms types here, so it's testable and reusable.
    public class ChatOrchestrator
    {
        private readonly ChatClientApi _api;
        public ChatOrchestrator(ChatClientApi api) => _api = api;

        // route on the selected type
        private static bool IsEventType(string t) => t is "MatchEvent" or "MatchEventList";
        private static bool IsStructuredType(string t) => t is "PlayerAppearance" or "PlayerList";

        public async Task<ClientResponse> HandleInputAsync(string input, string selectedType)
        {
            var text = input.Trim();

            if (LocalCommands.IsHelp(text))                       // ← short-circuit: no network
            {
                return new ClientResponse(ResponseKind.Help, LocalCommands.HelpText);
            }

            try
            {
                var request = new TypedPromptRequest(text, selectedType);
                string completion = await _api.SendMessageAsync("runTypedPrompt", request);

                if (IsEventType(selectedType))
                {
                    return new ClientResponse(ResponseKind.EventResult, completion);
                }

                if (IsStructuredType(selectedType))
                {
                    return new ClientResponse(ResponseKind.StructuredResult, completion);
                }

                return new ClientResponse(ResponseKind.FreeformText, completion);
            }
            catch (Exception ex)
            {
                return new ClientResponse(ResponseKind.Error, ex.Message);
            }
        }
    }
}
