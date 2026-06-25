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
            lstLog = new ListBox();
            rtbAiChat = new RichTextBox();
            txtInput = new TextBox();
            label1 = new Label();
            label2 = new Label();
            lblResultType = new Label();
            cmbResultType = new ComboBox();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // lstLog
            // 
            lstLog.FormattingEnabled = true;
            lstLog.Location = new Point(12, 718);
            lstLog.Name = "lstLog";
            lstLog.Size = new Size(1354, 404);
            lstLog.TabIndex = 1;
            // 
            // rtbAiChat
            // 
            rtbAiChat.DetectUrls = false;
            rtbAiChat.Location = new Point(12, 120);
            rtbAiChat.Name = "rtbAiChat";
            rtbAiChat.Size = new Size(1354, 346);
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
            label1.Location = new Point(12, 80);
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
            lblResultType.Size = new Size(204, 25);
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
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblStatus.Location = new Point(656, 46);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(98, 38);
            lblStatus.TabIndex = 12;
            lblStatus.Text = "Status";
            // 
            // UiForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1411, 1183);
            Controls.Add(lblStatus);
            Controls.Add(cmbResultType);
            Controls.Add(lblResultType);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtInput);
            Controls.Add(rtbAiChat);
            Controls.Add(lstLog);
            Name = "UiForm";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox lstLog;
        private RichTextBox rtbAiChat;
        private TextBox txtInput;
        private Label label1;
        private Label label2;
        private Label lblResultType;
        private ComboBox cmbResultType;
        private Label lblStatus;
    }
}
