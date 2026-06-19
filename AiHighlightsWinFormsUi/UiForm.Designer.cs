namespace WinFormsApp1
{
    partial class UiForm
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
            label1 = new Label();
            label2 = new Label();
            lblResultType = new Label();
            cmbResultType = new ComboBox();
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
            rtbAiChat.Location = new Point(12, 50);
            rtbAiChat.Name = "rtbAiChat";
            rtbAiChat.Size = new Size(1354, 416);
            rtbAiChat.TabIndex = 3;
            rtbAiChat.Text = "";
            // 
            // txtInput
            // 
            txtInput.Location = new Point(429, 532);
            txtInput.Multiline = true;
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(937, 112);
            txtInput.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = SystemColors.Control;
            label1.Location = new Point(11, 9);
            label1.Name = "label1";
            label1.Size = new Size(110, 25);
            label1.TabIndex = 7;
            label1.Text = "Chat Output";
            //
            // label2
            //
            label2.AutoSize = true;
            label2.Location = new Point(429, 483);
            label2.Name = "label2";
            label2.Size = new Size(95, 25);
            label2.TabIndex = 8;
            label2.Text = "Chat Input";
            //
            // lblResultType
            //
            lblResultType.AutoSize = true;
            lblResultType.Location = new Point(12, 483);
            lblResultType.Name = "lblResultType";
            lblResultType.TabIndex = 9;
            lblResultType.Text = "Result Type (Shift+Enter)";
            //
            // cmbResultType
            //
            cmbResultType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbResultType.Location = new Point(12, 532);
            cmbResultType.Name = "cmbResultType";
            cmbResultType.Size = new Size(400, 33);
            cmbResultType.TabIndex = 10;
            //
            // UiForm
            //
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1411, 1183);
            Controls.Add(cmbResultType);
            Controls.Add(lblResultType);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtInput);
            Controls.Add(rtbAiChat);
            Controls.Add(btnPlayHighlights);
            Controls.Add(lstLog);
            Controls.Add(btnStartProcessing);
            Name = "UiForm";
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
        private Label label1;
        private Label label2;
        private Label lblResultType;
        private ComboBox cmbResultType;
    }
}
