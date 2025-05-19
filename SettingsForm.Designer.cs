namespace RFIDScannerApp
{
    partial class SettingsForm
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
            mainPanel = new Panel();
            lblUri = new Label();
            txtUri = new TextBox();
            btnSave = new Button();
            titleLabel = new Label();
            logoPictureBox = new PictureBox();
            mainPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.BackColor = UIHelpers.HexToColor("#1e1e1e");
            mainPanel.Controls.Add(lblUri);
            mainPanel.Controls.Add(txtUri);
            mainPanel.Controls.Add(btnSave);
            mainPanel.Location = new Point(80, 150);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new Padding(10);
            mainPanel.Size = new Size(450, 140);
            mainPanel.TabIndex = 1;
            UIHelpers.RoundCorners(mainPanel, 15);
            // 
            // lblUri
            // 
            lblUri.Font = new Font("Arial", 10F);
            lblUri.ForeColor = Color.White;
            lblUri.Location = new Point(20, 30);
            lblUri.Name = "lblUri";
            lblUri.Size = new Size(100, 23);
            lblUri.TabIndex = 0;
            lblUri.Text = "Server URI";
            // 
            // txtUri
            // 
            txtUri.BackColor = UIHelpers.HexToColor("#707070");
            txtUri.ForeColor = Color.White;
            txtUri.BorderStyle = BorderStyle.FixedSingle;
            txtUri.Location = new Point(120, 25);
            txtUri.Name = "txtUri";
            txtUri.Size = new Size(300, 35);
            txtUri.TabIndex = 1;
            txtUri.TextChanged += TxtUri_TextChanged;
            // 
            // btnSave
            // 
            btnSave.FlatAppearance.BorderColor = UIHelpers.HexToColor("#027f80");
            btnSave.FlatAppearance.BorderSize = 1;
            btnSave.Enabled = false;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.BackColor = UIHelpers.HexToColor("#141414");
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(165, 82);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 30);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save";
            btnSave.Click += BtnSave_Click;
            btnSave.EnabledChanged += BtnSave_EnabledChanged;

            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Arial", 14F, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Location = new Point(215, 100);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(200, 30);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Settings Form";
            // Logo PictureBox
            logoPictureBox.Image = Image.FromFile("Resources/socoe.png"); // Use your actual resource name
            logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoPictureBox.BackColor = Color.Transparent;
            logoPictureBox.Location = new Point(200, 10); // Position near top-left
            logoPictureBox.Size = new Size(200, 100); // Adjust size as needed
            logoPictureBox.TabStop = false;
            // 
            // SettingsForm
            // 
            BackColor = UIHelpers.HexToColor("#141414");
            ClientSize = new Size(600, 330);
            Controls.Add(titleLabel);
            Controls.Add(mainPanel);
            Controls.Add(logoPictureBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private TextBox txtUri;
        private Button btnSave;
        private Label lblUri;
        private Label titleLabel;
        private Panel mainPanel;
        private PictureBox logoPictureBox;
    }
}