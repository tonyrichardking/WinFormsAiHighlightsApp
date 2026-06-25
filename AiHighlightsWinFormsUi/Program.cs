using AiHighlightsWinFormsUi;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace WinFormsApp1
{
    internal static class Program
    {
        public static string SourceVideoFilePath    { get; private set; } = "";
        public static string OutputClipDirPath      { get; private set; } = "";
        public static string OutputHighlightsDirPath { get; private set; } = "";
        public static string MediaDirPath           { get; private set; } = "";

        public static ChatClientApi TheAiChatClient { get; private set; }

        public static UiForm MainForm { get; private set; }

        // In-memory buffer for log messages produced before the UI is ready.
        private static int MaxLogBufferSize = 1000; // default, may be overridden from appsettings.json
        private static readonly object _logBufferLock = new object();
        // Use a fixed-size circular buffer (Queue) to store recent messages.
        private static Queue<string> _logBuffer = new Queue<string>();

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

            var paths    = config.GetSection("Paths");
            var appDir   = paths["AppDir"]   ?? "";
            var mediaDir = paths["MediaDir"] ?? "";

            SourceVideoFilePath     = Path.Combine(appDir, mediaDir, paths["MediaFileName"] ?? "");
            OutputClipDirPath       = Path.Combine(appDir, paths["ClipsDir"]      ?? "Clips");
            OutputHighlightsDirPath = Path.Combine(appDir, paths["HighlightsDir"] ?? "Highlights");
            MediaDirPath            = Path.Combine(appDir, mediaDir);

            // read optional logging configuration (max buffered log messages)
            var loggingSection = config.GetSection("Logging");
            if (int.TryParse(loggingSection["MaxLogBufferSize"], out var configuredSize) && configuredSize > 0)
            {
                MaxLogBufferSize = configuredSize;
            }

            ChatClientApi TheAiChatClient = new ChatClientApi(new HttpClient() 
            { 
                BaseAddress = new Uri("http://localhost:11190/aiChat/"),
                Timeout = TimeSpan.FromMinutes(5)
            });

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            var mainFormInstance = new UiForm(TheAiChatClient);
            RegisterMainForm(mainFormInstance);

            Application.Run(MainForm);
        }

        // https://stackoverflow.com/questions/2196097/elegant-log-window-in-winforms-c-sharp
        public static void LogToForm(string message)
        {
            lock (_logBufferLock)
            {
                if (MainForm is null)
                {
                    // buffer message until UI is ready; enforce max buffer size to avoid unbounded growth
                    if (_logBuffer.Count >= MaxLogBufferSize)
                    {
                        // drop the oldest message to make room
                        _logBuffer.Dequeue();
                    }
                    var tsMessage = $"[Buffered {DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                    _logBuffer.Enqueue(tsMessage);
                    Debug.WriteLine(tsMessage);
                    return;
                }
            }

            // MainForm is available; log directly on UI thread.
            MainForm.Log(message);
        }

        /// <summary>
        /// Registers the main UI form and flushes any buffered log messages to it.
        /// Call this as soon as the form instance is available.
        /// </summary>
        public static void RegisterMainForm(UiForm form)
        {
            if (form is null) return;

            MainForm = form;

            string[] buffered;
            lock (_logBufferLock)
            {
                buffered = _logBuffer.ToArray();
                _logBuffer.Clear();
            }

            foreach (var msg in buffered)
            {
                // Buffered messages already include a timestamp; use LogRaw to avoid double timestamps.
                MainForm.LogRaw(msg);
            }
        }
    }
}