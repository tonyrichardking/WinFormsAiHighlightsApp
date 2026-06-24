using AiHighlightsMcpServer.Prompt_Engineering;
using AiHighlightsMcpServer.Services;
using System.Text.Json.Serialization;

namespace AiHighlightsWinFormsUi.MediaPipeline
{
    // ---- Pipeline types (produced by CODE from MatchEvents, NOT emitted by the model) ----

    /// <summary>
    /// Map each event's instant to media time first, then pad there. 
    /// </summary>
    public record HighlightSegment(string Label, MediaTimecode In, MediaTimecode Out, MatchEvent Source);

    /// <summary>
    /// A reel: an ordered list of highlight segments.
    /// </summary>
    public record HighlightSegmentList(HighlightSegment[] Segments);
}
