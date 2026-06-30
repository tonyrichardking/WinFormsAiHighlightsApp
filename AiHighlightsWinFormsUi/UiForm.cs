using AiHighlightsMcpServer.Prompt_Engineering;
using AiHighlightsWinFormsUi;
using AiHighlightsWinFormsUi.MediaPipeline;
using AiHighlightsWinFormsUi.Orchestration;
using AiHighlightsWinFormsUi.Rendering;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace WinFormsApp1
{
    public partial class UiForm : Form
    {
        private readonly ChatClientApi theAiChatClient;
        private readonly ChatOrchestrator _orchestrator;

        public UiForm(ChatClientApi aiChatClient)
        {
            InitializeComponent();
            theAiChatClient = aiChatClient;
            _orchestrator = new ChatOrchestrator(theAiChatClient);
            ConfigureControls();

            InitialiseChat();

            //string aiIntroduction = Program.SendAndReadResponse(aiChatClient, "runPrompt", "Hello.  Can you introduce yourself please?").Result;
            //Log($"AI Introduction: {aiIntroduction}");
        }

        public void Log(string message)
        {
            List<string> messageLines = message.Split("\n").ToList();
            Log(messageLines);
        }

        public void Log(List<string> message)
        {
            if (InvokeRequired)
            {
                // Marshal the call to the UI thread to avoid cross-thread exceptions
                Invoke(new Action<List<string>>(Log), message);
                return;
            }

            lstLog.Items.Add($"\n {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: ");
            foreach (string line in message)
            {
                lstLog.Items.Add(line);
            }

            // Refresh once after all items have been added
            lstLog.Refresh();
        }

        // LogRaw: append lines exactly as provided, without adding an extra timestamp header.
        public void LogRaw(string message)
        {
            List<string> messageLines = message.Split("\n").ToList();
            LogRaw(messageLines);
        }

        public void LogRaw(List<string> message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<List<string>>(LogRaw), message);
                return;
            }

            foreach (string line in message)
            {
                lstLog.Items.Add(line);
            }

            lstLog.Refresh();
        }

        /// <summary>
        /// Initialise chat client properties, model, system prompt.
        /// </summary>
        public async void InitialiseChat()
        {
            // initialise the AiChatClient

            await theAiChatClient.SendMessageAsync("setModel", "Claude");       // gpt-oss:latest, Claude
            await theAiChatClient.SendMessageAsync("setSystemPrompt", "Sports");

            string selectedOptions = await theAiChatClient.SendMessageAsync("getOptions", "");
            // split the selectedOptions message into individual lines
            //List<string> lines = selectedOptions.Split("\n").ToList();

            Log($"{selectedOptions}");
        }

        /// <summary>
        /// Configures chat UI control properties and attaches input event handlers.
        /// </summary>
        private void ConfigureControls()
        {
            rtbAiChat.ReadOnly = true;
            rtbAiChat.BackColor = Color.White;
            rtbAiChat.BorderStyle = BorderStyle.None;
            rtbAiChat.Font = new System.Drawing.Font("Segoe UI", 11);

            cmbResultType.Items.Clear();
            cmbResultType.Items.AddRange(new object[] { "Auto", "MatchEventList", "PlayerList" });
            cmbResultType.SelectedIndex = 0;   // Auto by default

            txtInput.KeyDown += TxtInput_KeyDown;
        }

        private void AppendUserMessageToChatOutput(string text)
        {
            AppendStyledTextToChatOutput(
                $"You:\n{text}\n\n",
                Color.DarkBlue,
                new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold),
                HorizontalAlignment.Right);
        }

        private void AppendAssistantMessageToChatOutput(string text)
        {
            AppendStyledTextToChatOutput(
                $"Assistant:\n{text}",
                Color.DarkGreen,
                new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold),
                HorizontalAlignment.Left);
        }

        private void AppendStyledTextToChatOutput(string text, Color colour, System.Drawing.Font font, HorizontalAlignment alignment)
        {
            if (InvokeRequired)
            {
                Invoke(() =>
                    AppendStyledTextToChatOutput(text, colour, font, alignment));

                return;
            }

            rtbAiChat.SelectionStart = rtbAiChat.TextLength;
            rtbAiChat.SelectionLength = 0;
            rtbAiChat.SelectionColor = colour;
            rtbAiChat.SelectionFont = font;
            rtbAiChat.SelectionAlignment = alignment;
            rtbAiChat.AppendText(text);
            rtbAiChat.SelectionColor = rtbAiChat.ForeColor;
            rtbAiChat.ScrollToCaret();
        }

        private async void TxtInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || e.Shift)
            {
                return;
            }

            e.SuppressKeyPress = true;

            var promptText = txtInput.Text.Trim();
            if (promptText.Length == 0)
            {
                return;
            }

            AppendUserMessageToChatOutput(promptText);
            txtInput.Clear();
            SetBusy(true);

            var selectedType = cmbResultType.SelectedItem?.ToString() ?? "Auto";
            var progress = new Progress<PipelineStage>(ReportStage);   // callback runs on UI thread

            try
            {
                var response = await _orchestrator.HandleInputAsync(promptText, selectedType, progress);

                // route the response based on its kind, and render it in the chat output
                switch (response.Kind)
                {
                    case ResponseKind.Help:
                        AppendAssistantMessageToChatOutput(response.Text);
                        break;

                    case ResponseKind.EventResult:
                        AppendRouteMarker(InterpretationMarker(response.ChosenType));      // ← what the model understood
                        AppendTableToChatOutput(TableRenderer.Render(response.Events!.Events));
                        AppendRouteMarker("[reel playing]");
                        break;

                    case ResponseKind.StructuredResult:
                        AppendRouteMarker(InterpretationMarker(response.ChosenType));
                        var players = JsonSerializer.Deserialize<PlayerList>(
                            response.Text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        AppendTableToChatOutput(TableRenderer.Render(players!.Players));
                        break;

                    case ResponseKind.FreeformText:                                        // ← was empty
                        AppendRouteMarker(InterpretationMarker(response.ChosenType));
                        AppendAssistantMessageToChatOutput(response.Text);
                        break;

                    case ResponseKind.Error:                                               // ← was empty
                        AppendAssistantMessageToChatOutput($"Error: {response.Text}");
                        break;

                    default:
                        Log($"Unknown response kind: {response.Kind}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"Error during processing: {ex.Message}");
                AppendAssistantMessageToChatOutput($"Error: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private static string InterpretationMarker(string? chosenType)
        {
            if (string.IsNullOrEmpty(chosenType))
                return "[understood your request]";

            // Reuse the server-side descriptions if the client references that catalog;
            // otherwise a tiny local map. Keep it to ONE place.
            var phrase = SoccerResultTypeCatalog.Descriptions.TryGetValue(chosenType, out var d)
                ? d
                : chosenType;
            return $"[understood as: {phrase}]";
        }

        private void AppendTableToChatOutput(string table) =>
            AppendStyledTextToChatOutput($"{table}\n",
            Color.Black,
            new System.Drawing.Font("Consolas", 10),                       // fixed-width: the whole point
            HorizontalAlignment.Left);

        private void AppendRouteMarker(string text) =>
            AppendStyledTextToChatOutput($"{text}\n", Color.Gray,
                new System.Drawing.Font("Segoe UI", 8, FontStyle.Italic), HorizontalAlignment.Left);

        private void ReportStage(PipelineStage stage)
        {
            lblStatus.Text = stage switch
            {
                PipelineStage.Thinking => "Finding events…",
                PipelineStage.MappingTimes => "Mapping match time to video…",
                PipelineStage.CuttingClips => "Cutting clips…",
                PipelineStage.AssemblingReel => "Assembling reel…",
                PipelineStage.Ready => "Playing highlights.",
                PipelineStage.Failed => "Something went wrong.",
                _ => ""
            };
        }

        private void SetBusy(bool busy)
        {
            txtInput.Enabled = !busy;
            cmbResultType.Enabled = !busy;
            lblStatus.Visible = busy || lblStatus.Text == "Playing highlights.";
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }
    }
}




/*

        private async void TxtInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || e.Shift)
            {
                return;
            }

            e.SuppressKeyPress = true;

            var promptText = txtInput.Text.Trim();
            if (promptText.Length == 0)
            {
                return;
            }

            AppendUserMessageToChatOutput(promptText);
            txtInput.Clear();
            SetBusy(true);

            var selectedType = cmbResultType.SelectedItem?.ToString() ?? "Auto";
            var progress = new Progress<PipelineStage>(ReportStage);   // callback runs on UI thread

            try
            {
                var response = await _orchestrator.HandleInputAsync(promptText, selectedType, progress);

                // route the response based on its kind, and render it in the chat output
                switch (response.Kind)
                {
                    case ResponseKind.Help:
                        AppendAssistantMessageToChatOutput(response.Text);                  // raw JSON for now
                        break;
                    case ResponseKind.EventResult:
                        AppendTableToChatOutput(TableRenderer.Render(response.Events!.Events));   // MatchEvent[]
                        AppendRouteMarker("[reel playing]");
                        break;
                    case ResponseKind.StructuredResult:
                        // PlayerList isn't parsed upstream yet — deserialize here, then render the same way.
                        var players = JsonSerializer.Deserialize<PlayerList>(
                            response.Text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        AppendTableToChatOutput(TableRenderer.Render(players!.Players));
                        break;
                    case ResponseKind.FreeformText:

                        break;
                    case ResponseKind.Error:

                        break;
                    default:
                        Log($"Unknown response kind: {response.Kind}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"Error during processing: {ex.Message}");
                AppendAssistantMessageToChatOutput($"Error: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

 */
