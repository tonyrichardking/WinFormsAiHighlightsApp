using AiHighlightsMcpServer.Prompt_Engineering;

namespace AiHighlightsWinFormsUi.MediaPipeline
{
    /// How many seconds of lead-in and lead-out a highlight of a given type carries.
    //public record PaddingRule(int LeadInSeconds, int LeadOutSeconds);

    // Padding rules example: a goal wants the build-up and the celebration; a foul much less.
    //var paddingRules = new Dictionary<string, PaddingRule>
    //{
    //    ["Goal"] = new(15, 20),
    //    ["Foul"] = new(5, 5),
    //    ["YellowCard"] = new(8, 6),   // show the offence that earned it
    //    ["RedCard"] = new(10, 8),
    //};

    public class HighlightBuilder
    {
        private readonly MediaTimeMapper _mapper;
        private readonly TimeSpan _mediaDuration;                       // for clamping to the file
        private readonly IReadOnlyDictionary<string, PaddingRule> _rules;
        private readonly PaddingRule _default;

        public HighlightBuilder(MediaTimeMapper mapper, TimeSpan mediaDuration,
                                IReadOnlyDictionary<string, PaddingRule> rules, PaddingRule? @default = null)
        {
            _mapper = mapper;
            _mediaDuration = mediaDuration;
            _rules = rules;
            _default = @default ?? new PaddingRule(5, 5);
        }

        /// <summary>
        /// Given a list of match events, build a list of highlight segments with media in/out points.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public HighlightSegmentList Build(MatchEventList events)
        {
            var segments = new List<HighlightSegment>();

            foreach (var e in events.Events)
            {
                // 1. Anchor the instant on the media timeline.
                //    (This is where the mapper's "not in media" result pays off — see below.)
                var anchor = _mapper.OptaToMediaTime(e.Period, e.TimeMin, e.TimeSec).Offset;

                // 2. Per-type padding, expanded in simple media seconds...
                var rule = _rules.GetValueOrDefault(e.EventType, _default);
                var inPoint = anchor - TimeSpan.FromSeconds(rule.LeadInSeconds);
                var outPoint = anchor + TimeSpan.FromSeconds(rule.LeadOutSeconds);

                // 3. ...then clamped to the actual file bounds.
                if (inPoint < TimeSpan.Zero) inPoint = TimeSpan.Zero;
                if (outPoint > _mediaDuration) outPoint = _mediaDuration;

                var label = $"{e.EventType} — {e.PlayerName} - {e.Team} ({e.TimeMin}:{e.TimeSec:D2})";
                segments.Add(new HighlightSegment(label, new MediaTimecode(inPoint), new MediaTimecode(outPoint), e));
            }

            return new HighlightSegmentList(MergeOverlapping(segments).ToArray());
        }

        // Merge segments whose windows touch or overlap, so the reel never double-cuts
        // the same footage or leaves jarring micro-gaps.
        private static IEnumerable<HighlightSegment> MergeOverlapping(List<HighlightSegment> segments)
        {
            if (segments.Count == 0) yield break;

            var ordered = segments.OrderBy(s => s.In.Offset).ToList();
            var current = ordered[0];

            foreach (var next in ordered.Skip(1))
            {
                if (next.In.Offset <= current.Out.Offset)                       // overlap or touch
                {
                    var mergedOut = next.Out.Offset > current.Out.Offset ? next.Out : current.Out;
                    current = current with { Out = mergedOut, Label = $"{current.Label}; {next.Label}" };
                }
                else
                {
                    yield return current;
                    current = next;
                }
            }
            yield return current;
        }
    }
}
