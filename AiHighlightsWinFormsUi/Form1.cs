using AiHighlightsWinFormsUi;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using AiHighlightsMcpServer.Prompt_Engineering;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private readonly AiChatClient theAiChatClient;
        private string sourceVideoFilePath = Program.SourceVideoFilePath;

        public Form1(AiChatClient aiChatClient)
        {
            InitializeComponent();
            theAiChatClient = aiChatClient;
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

            txtInput.KeyDown += TxtInput_KeyDown;

            //btnSend.Click += BtnSend_Click;
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
            Player.PlayVideoFromShell(Program.FfPlayBatPath);
        }

        private async void btnStartChat_Click(object sender, EventArgs e)
        {
            string aiIntroduction = await theAiChatClient.SendMessageAsync("runPrompt", "Hello, please introduce yourself");
            Log($"AI Introduction: {aiIntroduction}");
        }

        private async void btnGetOptions_Click(object sender, EventArgs e)
        {
            string selectedOptions = await theAiChatClient.SendMessageAsync("getOptions", "");

            Log($"{selectedOptions}");
        }

        private async void TxtInput_KeyDown(object? sender, KeyEventArgs e)
        {
            string text = txtInput.Text.Trim();

            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;

                AppendUserMessageToChatOutput(text);
                txtInput.Clear();

                string completion = await theAiChatClient.SendMessageAsync("runPrompt", text);
                AppendAssistantMessageToChatOutput(completion);
            }
            // Shift+Enter: typed prompt — returns a structured GoalScorer result
            else if (e.KeyCode == Keys.Enter && e.Shift)
            {
                e.SuppressKeyPress = true;

                AppendUserMessageToChatOutput(text);
                txtInput.Clear();

                var typedRequest = new TypedPromptRequest(text, "GoalScorer");
                string completion = await theAiChatClient.SendMessageAsync("runTypedPrompt", typedRequest);
                AppendAssistantMessageToChatOutput(completion);
            }
        }
    }
}


/*

private async Task AppendAssistantResponseAsync(string prompt)
{
    BeginAssistantMessage();

    await foreach (string chunk in theAiClient.SendMessageAsync(prompt))
    {
        AppendAssistantChunk(chunk);
    }

    AppendNewLine();
}

private void BeginAssistantMessage()
{
    AppendStyledText(
        "Assistant:\n",
        Color.DarkGreen,
        new System.Drawing.Font("Consolas", 11, FontStyle.Bold),
        HorizontalAlignment.Left);
}


private void AppendAssistantChunk(string chunk)
{
    AppendStyledText(
        chunk,
        Color.Black,
        new System.Drawing.Font("Segoe UI", 11),
        HorizontalAlignment.Left);
}

*/