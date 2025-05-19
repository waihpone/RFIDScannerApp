namespace RFIDScannerApp
{
    partial class WriteForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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

        private void InitializeComponent()
        {
            writePanel = new Panel();
            btnWrite = new Button();
            btnBack = new Button();
            lblMessage = new Label();
            txtMessage = new TextBox();
            lblWriteStatus = new Label();
            titleLabel = new Label();
            logoPictureBox = new PictureBox();
            writePanel.SuspendLayout();
            SuspendLayout();
            // 
            // writePanel
            // 
            writePanel.BackColor = UIHelpers.HexToColor("#1e1e1e");
            writePanel.Controls.Add(btnWrite);
            writePanel.Controls.Add(btnBack);
            writePanel.Controls.Add(lblMessage);
            writePanel.Controls.Add(txtMessage);
            writePanel.Controls.Add(lblWriteStatus);
            writePanel.Location = new Point(80, 150);
            writePanel.Name = "writePanel";
            writePanel.Padding = new Padding(10);
            writePanel.Size = new Size(450, 230);
            writePanel.TabIndex = 0;
            UIHelpers.RoundCorners(writePanel, 15);
            // 
            // btnWrite
            // 
            btnWrite.FlatAppearance.BorderColor = UIHelpers.HexToColor("#027f80");
            btnWrite.FlatAppearance.BorderSize = 1;
            btnWrite.FlatStyle = FlatStyle.Flat;
            btnWrite.BackColor = UIHelpers.HexToColor("#141414");
            btnWrite.ForeColor = Color.White;
            btnWrite.Location = new Point(240, 181);
            btnWrite.Name = "btnWrite";
            btnWrite.Size = new Size(180, 35);
            btnWrite.TabIndex = 0;
            btnWrite.Text = "Write to Card";
            btnWrite.Click += BtnWrite_Click;
            // 
            // btnBack
            // 
            btnBack.FlatAppearance.BorderColor = UIHelpers.HexToColor("#027f80");
            btnBack.FlatAppearance.BorderSize = 1;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.BackColor = UIHelpers.HexToColor("#141414");
            btnBack.ForeColor = Color.White;
            btnBack.Location = new Point(20, 181);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(100, 35);
            btnBack.TabIndex = 1;
            btnBack.Text = "Back";
            btnBack.Click += BtnBack_Click;
            // 
            // lblMessage
            // 
            lblMessage.Font = new Font("Arial", 10F);
            lblMessage.ForeColor = Color.White;
            lblMessage.Location = new Point(20, 30);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new Size(100, 23);
            lblMessage.TabIndex = 2;
            lblMessage.Text = "Message";
            // 
            // txtMessage
            // 
            txtMessage.BackColor = UIHelpers.HexToColor("#707070");
            txtMessage.ForeColor = Color.White;
            txtMessage.BorderStyle = BorderStyle.FixedSingle;
            txtMessage.Location = new Point(120, 25);
            txtMessage.Multiline = true;
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(300, 80);
            txtMessage.TabIndex = 3;
            // 
            // lblWriteStatus
            // 
            lblWriteStatus.Font = new Font("Arial", 9F, FontStyle.Italic);
            lblWriteStatus.ForeColor = Color.Red;
            lblWriteStatus.Location = new Point(20, 115);
            lblWriteStatus.Name = "lblWriteStatus";
            lblWriteStatus.Size = new Size(400, 114);
            lblWriteStatus.TabIndex = 4;
            lblWriteStatus.Text = "Ready to write...";
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Arial", 14F, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Location = new Point(195, 100);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(220, 30);
            titleLabel.TabIndex = 1;
            titleLabel.Text = "Write to RFID/NFC";
            // Logo PictureBox
            logoPictureBox.Image = Image.FromFile("Resources/socoe.png"); // Use your actual resource name
            logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoPictureBox.BackColor = Color.Transparent;
            logoPictureBox.Location = new Point(200, 10); // Position near top-left
            logoPictureBox.Size = new Size(200, 100); // Adjust size as needed
            logoPictureBox.TabStop = false;
            // 
            // WriteForm
            // 
            BackColor = UIHelpers.HexToColor("#141414");
            ClientSize = new Size(600, 420);
            Controls.Add(titleLabel);
            Controls.Add(writePanel);
            Controls.Add(logoPictureBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "WriteForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Write to Card";
            writePanel.ResumeLayout(false);
            writePanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private Panel writePanel;
        private Button btnWrite;
        private Button btnBack;
        private Label lblMessage;
        private TextBox txtMessage;
        private Label lblWriteStatus;
        private Label titleLabel;
        private PictureBox logoPictureBox;
    }
}