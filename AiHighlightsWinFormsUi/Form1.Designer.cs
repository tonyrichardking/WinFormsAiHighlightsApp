namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnStartProcessing = new Button();
            lstLog = new ListBox();
            btnPlayHighlights = new Button();
            rtbAiChat = new RichTextBox();
            txtInput = new TextBox();
            btnStartChat = new Button();
            btnGetOptions = new Button();
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // btnStartProcessing
            // 
            btnStartProcessing.Location = new Point(12, 665);
            btnStartProcessing.Name = "btnStartProcessing";
            btnStartProcessing.Size = new Size(201, 34);
            btnStartProcessing.TabIndex = 0;
            btnStartProcessing.Text = "Start Processing";
            btnStartProcessing.UseVisualStyleBackColor = true;
            btnStartProcessing.Click += btnStartProcessing_Click;
            // 
            // lstLog
            // 
            lstLog.FormattingEnabled = true;
            lstLog.Location = new Point(12, 718);
            lstLog.Name = "lstLog";
            lstLog.Size = new Size(1354, 404);
            lstLog.TabIndex = 1;
            // 
            // btnPlayHighlights
            // 
            btnPlayHighlights.Location = new Point(12, 1128);
            btnPlayHighlights.Name = "btnPlayHighlights";
            btnPlayHighlights.Size = new Size(201, 34);
            btnPlayHighlights.TabIndex = 2;
            btnPlayHighlights.Text = "Play Highlights";
            btnPlayHighlights.UseVisualStyleBackColor = true;
            btnPlayHighlights.Click += btnPlayHighlights_Click;
            // 
            // rtbAiChat
            // 
            rtbAiChat.DetectUrls = false;
            rtbAiChat.Location = new Point(12, 98);
            rtbAiChat.Name = "rtbAiChat";
            rtbAiChat.Size = new Size(1354, 368);
            rtbAiChat.TabIndex = 3;
            rtbAiChat.Text = "";
            // 
            // txtInput
            // 
            txtInput.Location = new Point(12, 532);
            txtInput.Multiline = true;
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(1354, 112);
            txtInput.TabIndex = 4;
            // 
            // btnStartChat
            // 
            btnStartChat.Location = new Point(12, 12);
            btnStartChat.Name = "btnStartChat";
            btnStartChat.Size = new Size(201, 34);
            btnStartChat.TabIndex = 5;
            btnStartChat.Text = "Start Chat";
            btnStartChat.UseVisualStyleBackColor = true;
            btnStartChat.Click += btnStartChat_Click;
            // 
            // btnGetOptions
            // 
            btnGetOptions.Location = new Point(274, 12);
            btnGetOptions.Name = "btnGetOptions";
            btnGetOptions.Size = new Size(208, 34);
            btnGetOptions.TabIndex = 6;
            btnGetOptions.Text = "Get Options";
            btnGetOptions.UseVisualStyleBackColor = true;
            btnGetOptions.Click += btnGetOptions_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = SystemColors.Control;
            label1.Location = new Point(12, 60);
            label1.Name = "label1";
            label1.Size = new Size(110, 25);
            label1.TabIndex = 7;
            label1.Text = "Chat Output";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(11, 493);
            label2.Name = "label2";
            label2.Size = new Size(95, 25);
            label2.TabIndex = 8;
            label2.Text = "Chat Input";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1411, 1183);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnGetOptions);
            Controls.Add(btnStartChat);
            Controls.Add(txtInput);
            Controls.Add(rtbAiChat);
            Controls.Add(btnPlayHighlights);
            Controls.Add(lstLog);
            Controls.Add(btnStartProcessing);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnStartProcessing;
        private ListBox lstLog;
        private Button btnPlayHighlights;
        private RichTextBox rtbAiChat;
        private TextBox txtInput;
        private Button btnStartChat;
        private Button btnGetOptions;
        private Label label1;
        private Label label2;
    }
}
