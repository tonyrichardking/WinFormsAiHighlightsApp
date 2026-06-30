using AiHighlightsMcpServer.Prompt_Engineering;
using AiHighlightsWinFormsUi.MediaPipeline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace AiHighlightsWinFormsUi.Orchestration
{
    public enum ResponseKind { Help, EventResult, StructuredResult, FreeformText, Error }

    public record ClientResponse(ResponseKind Kind, string Text)
    {
        public MatchEventList? Events { get; init; }
    }

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
        private static PipelineWrapper _pipeline = new PipelineWrapper();
        private static string _sourceVideoFilePath => WinFormsApp1.Program.SourceVideoFilePath;
        private static string _mediaPath => WinFormsApp1.Program.MediaDirPath;

        // Mirror of the server's AutoResult. Payload stays as raw JSON until we know the type.
        private record AutoEnvelope(string ChosenType, JsonElement Payload);

        public record ClientResponse(ResponseKind Kind, string Text)
        {
            public MatchEventList? Events { get; init; }
            public string? ChosenType { get; init; }     // ← new: the server's decision
        }

        public async Task<ClientResponse> HandleInputAsync(string input, string selectedType, IProgress<PipelineStage> progress)
        {
            var text = input.Trim();
            if (LocalCommands.IsHelp(text))
                return new ClientResponse(ResponseKind.Help, LocalCommands.HelpText);

            try
            {
                progress.Report(PipelineStage.Thinking);
                var request = new TypedPromptRequest(text, selectedType);
                string completion = await _api.SendMessageAsync("runTypedPrompt", request);

                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var envelope = JsonSerializer.Deserialize<AutoEnvelope>(completion, opts);
                if (envelope is null)
                    return new ClientResponse(ResponseKind.Error, "Could not read the server response.");

                // Branch on what the SERVER chose, not the combo box.
                switch (envelope.ChosenType)
                {
                    case "MatchEventList":
                        {
                            var events = envelope.Payload.Deserialize<MatchEventList>(opts);
                            if (events is null || events.Events.Length == 0)
                                return new ClientResponse(ResponseKind.FreeformText, "No matching events found.")
                                { ChosenType = envelope.ChosenType };

                            await _pipeline.ProduceAndPlayAsync(events, _sourceVideoFilePath, _mediaPath, progress);
                            return new ClientResponse(ResponseKind.EventResult, completion)
                            { Events = events, ChosenType = envelope.ChosenType };
                        }

                    case "PlayerList":
                        return new ClientResponse(ResponseKind.StructuredResult, envelope.Payload.GetRawText())
                        { ChosenType = envelope.ChosenType };

                    case "Text":
                        return new ClientResponse(ResponseKind.FreeformText, envelope.Payload.GetString() ?? "")
                        { ChosenType = envelope.ChosenType };

                    default:
                        return new ClientResponse(ResponseKind.Error, $"Unknown result type: {envelope.ChosenType}");
                }
            }
            catch (Exception ex)
            {
                progress.Report(PipelineStage.Failed);
                return new ClientResponse(ResponseKind.Error, ex.Message);
            }
        }

        private MatchEventList? NormalizeToList(string json, string selectedType)
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            if (selectedType == "MatchEventList")
                return JsonSerializer.Deserialize<MatchEventList>(json, opts);

            var single = JsonSerializer.Deserialize<MatchEvent>(json, opts);   // singular
            return single is null ? null : new MatchEventList(new[] { single });
        }
    }
}




/*

        public async Task<ClientResponse> HandleInputAsync(string input, string selectedType, IProgress<PipelineStage> progress)
        {
            var text = input.Trim();
            if (LocalCommands.IsHelp(text))
                return new ClientResponse(ResponseKind.Help, LocalCommands.HelpText);

            try
            {
                progress.Report(PipelineStage.Thinking);
                var request = new TypedPromptRequest(text, selectedType);
                string completion = await _api.SendMessageAsync("runTypedPrompt", request);

                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var envelope = JsonSerializer.Deserialize<AutoEnvelope>(completion, opts);
                if (envelope is null)
                    return new ClientResponse(ResponseKind.Error, "Could not read the server response.");

                // Branch on what the SERVER chose, not the combo box.
                switch (envelope.ChosenType)
                {
                    case "MatchEventList":
                        {
                            var events = envelope.Payload.Deserialize<MatchEventList>(opts);
                            if (events is null || events.Events.Length == 0)
                                return new ClientResponse(ResponseKind.FreeformText, "No matching events found.");

                            await _pipeline.ProduceAndPlayAsync(events, _sourceVideoFilePath, _mediaPath, progress);
                            return new ClientResponse(ResponseKind.EventResult, completion) { Events = events };
                        }

                    case "PlayerList":
                        return new ClientResponse(ResponseKind.StructuredResult, envelope.Payload.GetRawText());

                    case "Text":
                        return new ClientResponse(ResponseKind.FreeformText, envelope.Payload.GetString() ?? "");

                    default:
                        return new ClientResponse(ResponseKind.Error, $"Unknown result type: {envelope.ChosenType}");
                }
            }
            catch (Exception ex)
            {
                progress.Report(PipelineStage.Failed);
                return new ClientResponse(ResponseKind.Error, ex.Message);
            }
        }


 */