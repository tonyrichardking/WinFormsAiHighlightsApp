using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;
using WinFormsApp1;

namespace AiHighlightsWinFormsUi.MediaPipeline
{
    public class SubClipper
    {
        public static void MakeClip(string inFile, string outFile, int start, int duration)
        {
            // ffmpeg -i input.mp4 -vf "drawtext=fontfile=/path/to/font.ttf:text='Stack Overflow':fontcolor=white:fontsize=24:box=1:boxcolor=black@0.5:boxborderw=5:x=(w-text_w)/2:y=(h-text_h)/2,drawtext=fontfile=/path/to/font.ttf:text='Bottom right text':fontcolor=black:fontsize=14:x=w-tw-10:y=h-th-10" -codec:a copy output.mp4


            // log the progress to the lstLog ListBox in Form1
            Program.LogToForm("Creating clip: " + outFile + " (start: " + start + "s, duration: " + duration + "s)");

            string configuredFfmpeg = @"ffmpeg.exe"; // Update this path to your ffmpeg executable if desired
            string ffmpegPath = configuredFfmpeg;

            var process = new Process();
            process.StartInfo.FileName = ffmpegPath;

            // to ensure frame-accuracy we don't use the -c copy option, but instead re-encode the video using a very fast preset and low quality settings - this should be much faster than a normal encode while still giving good enough quality for indexing purposes
            process.StartInfo.Arguments = $"-y -i \"{inFile}\" -ss {start} -t {duration} -c:v libx264 -preset fast -crf 28 -c:a copy \"{outFile}\"";
            //process.StartInfo.Arguments = $"-y -i \"{inFile}\" -ss {start} -t {duration} -c copy \"{outFile}\"";

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();
            process.OutputDataReceived += (s, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"FFmpeg failed with exit code {process.ExitCode}: {stderr}");
            }
        }

        public static void MakeClips(string sourceFile, string outputDir, string clipDefinitionJson)
        {
            Program.LogToForm("Creating clips...");

            Program.HghlightsDefinition highlights = JsonSerializer.Deserialize<Program.HghlightsDefinition>(clipDefinitionJson);

            // need to keep clips in order when re-assembled
            int clipNumber = 0;
            foreach (var clip in highlights.ClipList)
            {
                int start = clip.StartTimeSeconds;
                int duration = clip.DurationSeconds;
                string outFile = Path.Combine(outputDir, $"clip_{clipNumber++}_{start}_{duration}.mp4");
                MakeClip(sourceFile, outFile, start, duration);
            }

            Program.LogToForm("Finished");
        }

        public static void AssembleClips(List<string> clipFiles, string outputDir)
        {
            Program.LogToForm("Assembling clips...");

            string configuredFfmpeg = @"ffmpeg.exe"; // Update this path to your ffmpeg executable if desired

            // clip filenames have the form "clip_<number>_<start>_<duration>.mp4"
            // to keep the clips in order we need to order the filelist according to <number>
            clipFiles = clipFiles.OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f).Split('_')[1])).ToList();

            string fileListPath = Path.Combine(Path.GetTempPath(), "ffmpeg_file_list.txt");
            File.WriteAllLines(fileListPath, clipFiles.Select(f => $"file '{f}'"));

            string outputFilePath = Path.Combine(outputDir, $"Highlights.mp4");
            string ffmpegPath = configuredFfmpeg;

            var process = new Process();
            process.StartInfo.FileName = ffmpegPath;
            process.StartInfo.Arguments = $"-y -f concat -safe 0 -i \"{fileListPath}\" -c copy \"{outputFilePath}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();
            process.OutputDataReceived += (s, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            File.Delete(fileListPath);
            if (process.ExitCode != 0)
            {
                throw new Exception($"FFmpeg failed with exit code {process.ExitCode}: {stderr}");
            }

            Program.LogToForm("Finished");
        }
    }
}


/*

                // build all the required files

                List<string> allFiles = Directory.GetFiles(htmlInputDirPath).Order().ToList();

                Directory.CreateDirectory(chunkOutputDirPath);
                Directory.CreateDirectory(jsonOutputDirPath);
                Directory.CreateDirectory(cleanedSchemaOutputDirPath);
                Directory.CreateDirectory(mapOutputDirPath);

                if (deleteFilesOnStart)
                {
                    DirectoryInfo di;

                    di = new DirectoryInfo(chunkOutputDirPath);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }

                    di = new DirectoryInfo(jsonOutputDirPath);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }

                    di = new DirectoryInfo(cleanedSchemaOutputDirPath);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }

                    di = new DirectoryInfo(mapOutputDirPath);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                }

*/

/*

SOURCE_DIR = 'C:\\Projects\\Experiments_2026\\FunWithAiSoccerHighlights\\ManU v Brighton Media'
INPUT_FILE = 'FULL MATCH  Manchester United v Brighton and Hove Albion  Third Round  Emirates FA Cup 2025-26.mp4'
CLIP_DIR = 'C:\\Users\\tking\\Desktop\\ETL for AI\\Football Videos\\Infront\\Clips\\'
CLIP_LEN_MINS = 30

clipLengthInSecs = CLIP_LEN_MINS * 60

#------------------------------------------------------------------------------ 
# reduce resolution for indexing performance
def makeClip(inFile, outFile, start, duration):
    ffmpeg = (
        FFmpeg()
        .option("y")        # overwrite file
        .input(inFile)
        .output(
            outFile,
            ss=start, 
            t=duration,
        )
    )

    ffmpeg.execute()

#------------------------------------------------------------------------------
def getVideoProperties(sourceFile):
    ffprobe = FFmpeg(executable="ffprobe").input(
        sourceFile,
        print_format="json", # ffprobe will output the results in JSON format
        show_streams=None,
    )

    media = json.loads(ffprobe.execute())
    properties = media['streams'][0]

    return properties

#------------------------------------------------------------------------------

# set up logging

logFilePath = os.path.join(CLIP_DIR, "clipLog.txt")

logging.basicConfig(
    filename = logFilePath,
    level = logging.INFO,
    encoding = "utf-8",
    filemode = "a",
    format = "{asctime} - {levelname} - {message}",
    style = "{",
    datefmt = "%Y-%m-%d %H:%M",
)

logger = logging.getLogger(__name__)

# start processing

print("Starting Mpeg splitter")
logger.info("Starting Mpeg splitter")

#entries = os.listdir(SOURCE_DIR)
entries = [INPUT_FILE]

for entry in entries:
    path = os.path.join(SOURCE_DIR, entry)
    
    if os.path.isfile(path):
        logger.info(f"Started processing source file {path}")

        entryBits = entry.split('_')
        clipNameStem = "Infront_" + entryBits[4] + '-' + entryBits[5] + '-' + entryBits[6] + '-' + entryBits[3]
        videoProperties = getVideoProperties(path)
        durationString = videoProperties['duration'].split('.')[0]
        sourceVideoLengthInSecs = int(durationString)
        clipStartTimeInSecs = 0
        clipCount = 0

        while clipStartTimeInSecs < sourceVideoLengthInSecs:
            logger.info(f"Creating clip {clipCount} from source file {entry}")
            outFile = os.path.join(CLIP_DIR, clipNameStem + '-' + str(clipCount) + ".mp4") 
            makeClip(path, outFile, clipStartTimeInSecs, clipLengthInSecs)

            logger.info(f"start={clipStartTimeInSecs}s, duration={videoProperties["duration"]}s, width={videoProperties["width"]}, height={videoProperties["height"]}")
            logger.info(f"Clip written to {outFile}")

            clipStartTimeInSecs += clipLengthInSecs
            clipCount += 1

        logger.info(f"Finished processing source file {path}")


print("Finished Mpeg splitter")
logger.info("Finished")


 */