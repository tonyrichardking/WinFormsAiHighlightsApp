using AiHighlightsMcpServer.Prompt_Engineering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using WinFormsApp1;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AiHighlightsWinFormsUi.MediaPipeline
{
    public enum PipelineStage { Thinking, MappingTimes, CuttingClips, AssemblingReel, Ready, Failed }
    public record PaddingRule(int LeadInSeconds, int LeadOutSeconds);

    public class PipelineWrapper
    {
        // Example: a goal wants the build-up and the celebration; a foul much less.
        public static Dictionary<string, PaddingRule> _paddingRules = new Dictionary<string, PaddingRule>
        {
            ["Goal"] = new(15, 15),
            ["Foul"] = new(5, 5),
            ["Card"] = new(10, 10),   // show the offence that earned it
        };

        public async Task<string> ProduceAndPlayAsync(MatchEventList events, string sourceVideoFilePath, string mediaPath, IProgress<PipelineStage> progress)
        {
            progress.Report(PipelineStage.MappingTimes);
            string sidecarPath = Path.Combine(mediaPath, "ManU v Brighton Media.timemap.json");
            var mapper = MediaTimeMapper.FromSidecarFile(sidecarPath);
            //var builder = new HighlightBuilder(mapper, TotalMediaDuration(mapper.Config), _paddingRules);
            TimeSpan mediaDurationTimeSpan = TimeSpan.FromSeconds(ManuVsBrightonMapperContext.mediaDurationSeconds);
            var builder = new HighlightBuilder(mapper, mediaDurationTimeSpan, _paddingRules);
            HighlightSegmentList reel = builder.Build(events);     // instants → padded, clamped, merged segments

            progress.Report(PipelineStage.CuttingClips);
            var clips = new List<string>();
            foreach (var seg in reel.Segments)
            {
                clips.Add(await SubClipper.CutClipAsync(sourceVideoFilePath, seg));   // -ss/-to, -c copy
            }

            progress.Report(PipelineStage.AssemblingReel);
            var reelPath = await SubClipper.AssembleAsync(clips);

            progress.Report(PipelineStage.Ready);

            // Play the highlight reel fire-and-forget style
            if (true)
            {
                // fire-and-forget
                Process.Start(new ProcessStartInfo("ffplay", $"-autoexit \"{reelPath}\"") { UseShellExecute = false });
            }
            else
            {
                // doesn't return until ffplay's window closes
                await SubClipper.ProcessRunner.RunAsync("ffplay", $"-autoexit \"{reelPath}\"");
            }

            return reelPath;

            //await MediaPlayer.PlayAsync(reelPath);
        }
    }
}
