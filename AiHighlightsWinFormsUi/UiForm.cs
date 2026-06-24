using AiHighlightsMcpServer.Prompt_Engineering;
using AiHighlightsWinFormsUi;
using AiHighlightsWinFormsUi.MediaPipeline;
using AiHighlightsWinFormsUi.Orchestration;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace WinFormsApp1
{
    public partial class UiForm : Form
    {
        private readonly ChatClientApi theAiChatClient;
        private readonly ChatOrchestrator _orchestrator;
        private string sourceVideoFilePath = Program.SourceVideoFilePath;

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
            lstLog.Items.Add($"\n {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: ");
            foreach (string line in message)
            {
                lstLog.Items.Add(line);
                lstLog.Refresh();
            }
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

            cmbResultType.Items.Add("MatchEventList");
            cmbResultType.Items.Add("PlayerList");
            cmbResultType.SelectedIndex = 0;

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

        private void AppendNewLineToChatOutput()
        {
            AppendStyledTextToChatOutput(
                "\n",
                Color.Black,
                new System.Drawing.Font("Segoe UI", 11),
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

        private async void btnStartProcessing_Click(object sender, EventArgs e)
        {
            // ensure the directories exist
            Directory.CreateDirectory(Program.OutputClipDirPath);
            Directory.CreateDirectory(Program.OutputHighlightsDirPath);

            // delete any existing clips in the output directories
            foreach (string file in Directory.GetFiles(Program.OutputClipDirPath))
            {
                File.Delete(file);
            }

            foreach (string file in Directory.GetFiles(Program.OutputHighlightsDirPath))
            {
                File.Delete(file);
            }

            SubClipper.MakeClips(sourceVideoFilePath, Program.OutputClipDirPath, Program.ExampleClipDefinitionJson);
            SubClipper.AssembleClips(Directory.GetFiles(Program.OutputClipDirPath, "*.mp4").ToList(), Program.OutputHighlightsDirPath);
        }

        private void btnPlayHighlights_Click(object sender, EventArgs e)
        {
            MediaPlayer.PlayVideoFromShell(Program.FfPlayBatPath);
        }

        private async void TxtInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || e.Shift)
            {
                return;
            }

            e.SuppressKeyPress = true;

            var text = txtInput.Text.Trim();
            if (text.Length == 0)
            {
                return;
            }

            AppendUserMessageToChatOutput(text);
            txtInput.Clear();

            var selectedType = cmbResultType.SelectedItem?.ToString() ?? "MatchEventList";
            var response = await _orchestrator.HandleInputAsync(text, selectedType);

            // route the response based on its kind, and render it in the chat output
            switch (response.Kind)
            {
                case ResponseKind.Help:
                    
                    break;
                case ResponseKind.EventResult:
                    AppendAssistantMessageToChatOutput(response.Text);                  // raw JSON for now
                    AppendRouteMarker("[pipeline trigger goes here — next iteration]"); // the seam, made visible
                    break;
                case ResponseKind.StructuredResult:
                    
                    break;
                case ResponseKind.FreeformText:
                    
                    break;
                case ResponseKind.Error:
                    
                    break;
                default:
                    Log($"Unknown response kind: {response.Kind}");
                    break;
            }

            RenderResponse(response);
        }

        private void RenderResponse(ClientResponse response)
        {
            AppendRouteMarker($"[routed as: {response.Kind}]");   // the observable bit
            AppendAssistantMessageToChatOutput(
                response.Kind == ResponseKind.Error ? $"Error: {response.Text}" : response.Text);
        }

        private void AppendRouteMarker(string text) =>
            AppendStyledTextToChatOutput($"{text}\n", Color.Gray,
                new System.Drawing.Font("Segoe UI", 8, FontStyle.Italic), HorizontalAlignment.Left);
    }
}




/*

        private async void TxtInput_KeyDown(object? sender, KeyEventArgs e)
        {
            string text = txtInput.Text.Trim();

            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                // Enter: typed prompt — returns a structured GoalScorer result

                e.SuppressKeyPress = true;

                AppendUserMessageToChatOutput(text);
                txtInput.Clear();

                var selectedType = cmbResultType.SelectedItem?.ToString() ?? "MatchEventList";
                var typedRequest = new TypedPromptRequest(text, selectedType);
                string completion = await theAiChatClient.SendMessageAsync("runTypedPrompt", typedRequest);
                AppendAssistantMessageToChatOutput(completion);
            }
            else if (e.KeyCode == Keys.Enter && e.Shift)
            {
                // Shift+Enter: regular prompt — returns a simple text response

                e.SuppressKeyPress = true;

                AppendUserMessageToChatOutput(text);
                txtInput.Clear();

                string completion = await theAiChatClient.SendMessageAsync("runPrompt", text);
                AppendAssistantMessageToChatOutput(completion);
            }
        }

 * */
