using AiHighlightsWinFormsUi;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.Json;

namespace WinFormsApp1
{
    internal static class Program
    {
        public static string SourceVideoFilePath    { get; private set; } = "";
        public static string OutputClipDirPath      { get; private set; } = "";
        public static string OutputHighlightsDirPath { get; private set; } = "";
        public static string FfPlayBatPath          { get; private set; } = "";

        public static ChatClientApi TheAiChatClient { get; private set; }

        public static UiForm MainForm { get; private set; }

        public static string ExampleClipDefinitionJson { get; private set; }

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
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var paths = config.GetSection("Paths");
            SourceVideoFilePath     = paths["SourceVideoFilePath"]     ?? "";
            OutputClipDirPath       = paths["OutputClipDirPath"]       ?? "";
            OutputHighlightsDirPath = paths["OutputHighlightsDirPath"] ?? "";
            FfPlayBatPath           = paths["FfPlayBatPath"]           ?? "";

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

            ExampleClipDefinitionJson = JsonSerializer.Serialize(Highlights, new JsonSerializerOptions() { WriteIndented = true });

            // -----------------------------------------------------------

            ChatClientApi TheAiChatClient = new ChatClientApi(new HttpClient() { BaseAddress = new Uri("http://localhost:11190/aiChat/") });

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            MainForm = new UiForm(TheAiChatClient);          

            Application.Run(MainForm);
        }

        // https://stackoverflow.com/questions/2196097/elegant-log-window-in-winforms-c-sharp
        public static void LogToForm(string message)
        {
            MainForm.Log(message);
        }
    }
}