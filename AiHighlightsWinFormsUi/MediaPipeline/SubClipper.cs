using System.Diagnostics;
using System.IO;
using System.Linq;
using WinFormsApp1;

namespace AiHighlightsWinFormsUi.MediaPipeline
{
    public class SubClipper
    {
        public static class ProcessRunner
        {
            /// <summary>
            /// Helper for the FFMpeg calls
            /// </summary>
            /// <param name="exe"></param>
            /// <param name="args"></param>
            /// <param name="ct"></param>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"></exception>
            public static async Task RunAsync(string exe, string args, CancellationToken ct = default)
            {
                using var p = new Process
                {
                    StartInfo = new ProcessStartInfo(exe, args)
                    { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true }
                };
                p.Start();
                string stderr = await p.StandardError.ReadToEndAsync(ct);   // read before WaitForExit (deadlock guard)
                await p.WaitForExitAsync(ct);
                if (p.ExitCode != 0)
                    throw new InvalidOperationException($"{exe} failed ({p.ExitCode}): {stderr}");
            }
        }

        // Keeps iteration-4's SubClipper.CutClipAsync / AssembleAsync compiling unchanged:
        private static Task RunFfmpegAsync(string args, CancellationToken ct = default)
            => ProcessRunner.RunAsync("ffmpeg", args, ct);

        public static async Task<string> CutClipAsync(string sourcePath, HighlightSegment seg)
        {
            var outPath = Path.Combine(Program.OutputClipDirPath, $"clip_{Guid.NewGuid():N}.mp4");
            // -ss before -i = fast seek; -c copy = stream-copy, no re-encode (your GOP25 keyframes align)
            var args = $"-y -ss {seg.In.ToFfmpegTimecode()} -to {seg.Out.ToFfmpegTimecode()} " +
                       $"-i \"{sourcePath}\" -c copy \"{outPath}\"";
            await RunFfmpegAsync(args);
            return outPath;
        }

        public static async Task<string> AssembleAsync(IReadOnlyList<string> clipPaths)
        {
            if (clipPaths.Count == 0)
                throw new InvalidOperationException("No clips to assemble.");

            var reelPath = Path.Combine(Program.OutputHighlightsDirPath, $"reel_{Guid.NewGuid():N}.mp4");

            // The concat demuxer reads a text file listing the inputs, one per line:
            //   file 'C:/clips/clip_abc.mp4'
            var listPath = Path.Combine(Path.GetTempPath(), $"concat_{Guid.NewGuid():N}.txt");
            await File.WriteAllLinesAsync(listPath, clipPaths.Select(ToConcatLine));

            try
            {
                // -f concat   : use the concat demuxer
                // -safe 0     : allow absolute paths (the demuxer rejects them otherwise)
                // -c copy     : stream-copy, no re-encode
                var args = $"-y -f concat -safe 0 -i \"{listPath}\" -c copy \"{reelPath}\"";
                await RunFfmpegAsync(args);
                return reelPath;
            }
            finally
            {
                // Tidy the temp manifest whether assembly succeeded or threw.
                if (File.Exists(listPath)) File.Delete(listPath);
            }
        }

        // One manifest line. Forward slashes sidestep Windows backslash-escaping headaches
        // (ffmpeg accepts them on Windows), and a literal single quote becomes '\'' .
        private static string ToConcatLine(string path)
        {
            var normalized = path.Replace('\\', '/').Replace("'", "'\\''");
            return $"file '{normalized}'";
        }
    }
}