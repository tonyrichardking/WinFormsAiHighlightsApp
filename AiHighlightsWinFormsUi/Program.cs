using AiHighlightsWinFormsUi;
using System.Diagnostics;
using System.Text.Json;

namespace WinFormsApp1
{
    internal static class Program
    {
        public const string SourceVideoFilePath = @"C:\Projects\Experiments_2026\FunWithAiSoccerHighlights\ManU v Brighton Media\ManU v Brighton 854x480 GOP25.mp4";
        public const string OutputClipDirPath = @"C:\Projects\Experiments_2026\FunWithAiSoccerHighlights\Clips";
        public const string OutputHighlightsDirPath = @"C:\Projects\Experiments_2026\FunWithAiSoccerHighlights\Highlights";

        public static AiChatClient TheAiChatClient { get; private set; }

        public static Form1 MainForm { get; private set; }

        public static string exampleClipDefinitionJson { get; private set; }

        public class HghlightsDefinition()
        {
            public string MatchDescription { get; set; }
            public List<ClipDefinition> ClipList { get; set; } = new List<ClipDefinition>();
        }

        public class ClipDefinition()
        {
            public int StartTimeSeconds { get; set; }
            public int DurationSeconds { get; set; }
            public string Label { get; set; }
            public string Detail { get; set; }
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // build example clip definition data

            HghlightsDefinition Highlights = new HghlightsDefinition()
            {
                MatchDescription = "Manchester United vs Nottingham Forest",
                ClipList = new List<ClipDefinition>()
            };

            Highlights.ClipList.Add(new ClipDefinition() 
            { 
                StartTimeSeconds = 11*60 + 10, 
                DurationSeconds = 10,
                Label = "First Brighton Goal",
                Detail = "Brajan Gruda opens the scoring in the 12th minute against the run of play."
            });

            Highlights.ClipList.Add(new ClipDefinition()
            {
                StartTimeSeconds = 65*60 + 50,
                DurationSeconds = 15,
                Label = "Second Brighton Goal",
                Detail = "Danny Welbeck scores the second goal."
            });

            Highlights.ClipList.Add(new ClipDefinition()
            {
                StartTimeSeconds = 86*60 + 20,
                DurationSeconds = 15,
                Label = "First Manchester United Goal",
                Detail = "Benjamin Šeško. scores Man U's first goal."
            });

            exampleClipDefinitionJson = JsonSerializer.Serialize(Highlights, new JsonSerializerOptions() { WriteIndented = true });

            // -----------------------------------------------------------

            AiChatClient TheAiChatClient = new AiChatClient(new HttpClient() { BaseAddress = new Uri("http://localhost:11190/aiChat/") });

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            MainForm = new Form1(TheAiChatClient);          

            Application.Run(MainForm);
        }

        // https://stackoverflow.com/questions/2196097/elegant-log-window-in-winforms-c-sharp
        public static void LogToForm(string message)
        {
            MainForm.Log(message);
        }
    }
}