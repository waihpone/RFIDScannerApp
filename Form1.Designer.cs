using System.Drawing.Drawing2D;

namespace RFIDScannerApp
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            adminPanel = new Panel();
            adminID = new Label();
            adminIDTextBox = new TextBox();
            adminStatusLabel = new Label();
            btnSettings = new Button();
            mainPanel = new Panel();
            adminNameTextBox = new TextBox();
            deauthenticateButton = new Button();
            userID = new Label();
            userIDTextBox = new TextBox();
            Scanned_RFID = new Label();
            txtRFID = new TextBox();
            lblStatus = new Label();
            btnWriteData = new Button();
            btnResetUser = new Button();
            titleLabel = new Label();
            logoPictureBox = new PictureBox();
            adminPanel.SuspendLayout();
            mainPanel.SuspendLayout();
            SuspendLayout();
            // 
            // adminPanel
            // 
            adminPanel.BackColor = UIHelpers.HexToColor("#1e1e1e");
            adminPanel.Controls.Add(adminID);
            adminPanel.Controls.Add(adminIDTextBox);
            adminPanel.Controls.Add(adminStatusLabel);
            adminPanel.Controls.Add(btnSettings);
            adminPanel.Location = new Point(80, 150);
            adminPanel.Name = "adminPanel";
            adminPanel.Padding = new Padding(10);
            adminPanel.Size = new Size(450, 230);
            adminPanel.TabIndex = 2;
            UIHelpers.RoundCorners(adminPanel, 15);
            // 
            // adminID
            // 
            adminID.Font = new Font("Arial", 10F);
            adminID.ForeColor = Color.White;
            adminID.Location = new Point(20, 75);
            adminID.Name = "adminID";
            adminID.Size = new Size(100, 23);
            adminID.TabIndex = 0;
            adminID.Text = "Admin";
            // 
            // adminIDTextBox
            // 
            adminIDTextBox.BackColor = UIHelpers.HexToColor("#707070");
            adminIDTextBox.ForeColor = Color.White;
            adminIDTextBox.BorderStyle = BorderStyle.FixedSingle;
            adminIDTextBox.Location = new Point(120, 70);
            adminIDTextBox.Name = "adminIDTextBox";
            adminIDTextBox.ReadOnly = true;
            adminIDTextBox.Size = new Size(300, 35);
            adminIDTextBox.TabIndex = 1;
            // 
            // adminStatusLabel
            // 
            adminStatusLabel.Font = new Font("Arial", 9F, FontStyle.Italic);
            adminStatusLabel.ForeColor = Color.Red;
            adminStatusLabel.Location = new Point(20, 110);
            adminStatusLabel.Name = "adminStatusLabel";
            adminStatusLabel.Size = new Size(400, 30);
            adminStatusLabel.TabIndex = 2;
            adminStatusLabel.Text = "Waiting for admin scan...";
            // 
            // btnSettings
            // 
            btnSettings.FlatAppearance.BorderColor = UIHelpers.HexToColor("#027f80");
            btnSettings.FlatAppearance.BorderSize = 1;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.BackColor = UIHelpers.HexToColor("#141414");
            btnSettings.ForeColor = Color.White;
            btnSettings.Location = new Point(320, 170);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(100, 35);
            btnSettings.TabIndex = 6;
            btnSettings.Text = "Settings";
            btnSettings.Click += BtnSettings_Click;
            // 
            // mainPanel
            // 
            mainPanel.BackColor = UIHelpers.HexToColor("#1e1e1e");
            mainPanel.Controls.Add(adminNameTextBox);
            mainPanel.Controls.Add(deauthenticateButton);
            mainPanel.Controls.Add(userID);
            mainPanel.Controls.Add(userIDTextBox);
            mainPanel.Controls.Add(Scanned_RFID);
            mainPanel.Controls.Add(txtRFID);
            mainPanel.Controls.Add(lblStatus);
            mainPanel.Controls.Add(btnWriteData);
            mainPanel.Controls.Add(btnResetUser);
            mainPanel.Location = new Point(80, 150);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new Padding(10);
            mainPanel.Size = new Size(450, 230);
            mainPanel.TabIndex = 3;
            mainPanel.Visible = false;
            UIHelpers.RoundCorners(mainPanel, 15);
            // 
            // adminNameTextBox
            // 
            adminNameTextBox.BackColor = UIHelpers.HexToColor("#1e1e1e");
            adminNameTextBox.ForeColor = Color.White;
            adminNameTextBox.BorderStyle = BorderStyle.None;
            adminNameTextBox.Location = new Point(20, 17);
            adminNameTextBox.Name = "adminNameTextBox";
            adminNameTextBox.ReadOnly = true;
            adminNameTextBox.Size = new Size(260, 20);
            adminNameTextBox.TabIndex = 0;
            adminNameTextBox.TextAlign = HorizontalAlignment.Right;
            // 
            // deauthenticateButton
            // 
            deauthenticateButton.FlatAppearance.BorderColor = UIHelpers.HexToColor("#dc3545");
            deauthenticateButton.FlatAppearance.BorderSize = 1;
            deauthenticateButton.FlatStyle = FlatStyle.Flat;
            deauthenticateButton.BackColor = UIHelpers.HexToColor("#dc3545");
            deauthenticateButton.ForeColor = Color.White;
            deauthenticateButton.Font = new Font("Arial", 8F);
            deauthenticateButton.Location = new Point(290, 12);
            deauthenticateButton.Name = "deauthenticateButton";
            deauthenticateButton.Size = new Size(130, 35);
            deauthenticateButton.TabIndex = 1;
            deauthenticateButton.Text = "Deauthenticate";
            deauthenticateButton.Click += deauthenticateButton_Click;
            // 
            // userID
            // 
            userID.Font = new Font("Arial", 10F);
            userID.ForeColor = Color.White;
            userID.Location = new Point(20, 70);
            userID.Name = "userID";
            userID.Size = new Size(100, 23);
            userID.TabIndex = 0;
            userID.Text = "User";
            // 
            // userIDTextBox
            // 
            userIDTextBox.BackColor = UIHelpers.HexToColor("#707070");
            userIDTextBox.ForeColor = Color.White;
            userIDTextBox.BorderStyle = BorderStyle.FixedSingle;
            userIDTextBox.Location = new Point(120, 65);
            userIDTextBox.Name = "userIDTextBox";
            userIDTextBox.ReadOnly = true;
            userIDTextBox.Size = new Size(300, 27);
            userIDTextBox.TabIndex = 1;
            // 
            // Scanned_RFID
            // 
            Scanned_RFID.Font = new Font("Arial", 10F);
            Scanned_RFID.ForeColor = Color.White;
            Scanned_RFID.Location = new Point(20, 105);
            Scanned_RFID.Name = "Scanned_RFID";
            Scanned_RFID.Size = new Size(100, 23);
            Scanned_RFID.TabIndex = 2;
            Scanned_RFID.Text = "Asset";
            // 
            // txtRFID
            // 
            txtRFID.BackColor = UIHelpers.HexToColor("#707070");
            txtRFID.ForeColor = Color.White;
            txtRFID.BorderStyle = BorderStyle.FixedSingle;
            txtRFID.Location = new Point(120, 100);
            txtRFID.Name = "txtRFID";
            txtRFID.ReadOnly = true;
            txtRFID.Size = new Size(300, 27);
            txtRFID.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Arial", 9F, FontStyle.Italic);
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(20, 140);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(400, 30);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Waiting for scan...";
            // 
            // btnWriteData
            // 
            btnWriteData.FlatAppearance.BorderColor = UIHelpers.HexToColor("#027f80");
            btnWriteData.FlatAppearance.BorderSize = 1;
            btnWriteData.FlatStyle = FlatStyle.Flat;
            btnWriteData.BackColor = UIHelpers.HexToColor("#141414");
            btnWriteData.ForeColor = Color.White;
            btnWriteData.Location = new Point(320, 170);
            btnWriteData.Name = "btnWriteData";
            btnWriteData.Size = new Size(100, 35);
            btnWriteData.TabIndex = 5;
            btnWriteData.Text = "Write Data";
            btnWriteData.Click += BtnWriteData_Click;
            //
            // btnResetUser
            //
            btnResetUser.FlatAppearance.BorderColor = UIHelpers.HexToColor("#dc3545");
            btnResetUser.FlatAppearance.BorderSize = 1;
            btnResetUser.FlatStyle = FlatStyle.Flat;
            btnResetUser.BackColor = UIHelpers.HexToColor("#dc3545");
            btnResetUser.ForeColor = Color.White;
            btnResetUser.Location = new Point(20, 170);
            btnResetUser.Name = "btnResetUser";
            btnResetUser.Size = new Size(100, 35);
            btnResetUser.TabIndex = 6;
            btnResetUser.Text = "Reset User";
            btnResetUser.Click += BtnResetUser_Click;
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Arial", 14F, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Location = new Point(190, 100);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(200, 30);
            titleLabel.TabIndex = 1;
            titleLabel.Text = "RFID/NFC Scanner";
            // Logo PictureBox
            logoPictureBox.Image = Image.FromFile("Resources/socoe.png"); // Use your actual resource name
            logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoPictureBox.BackColor = Color.Transparent;
            logoPictureBox.Location = new Point(200, 10); // Position near top-left
            logoPictureBox.Size = new Size(200, 100); // Adjust size as needed
            logoPictureBox.TabStop = false;
            // 
            // Form1
            // 
            BackColor = UIHelpers.HexToColor("#141414");
            ClientSize = new Size(600, 420);
            Controls.Add(titleLabel);
            Controls.Add(adminPanel);
            Controls.Add(mainPanel);
            Controls.Add(logoPictureBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "RFID/NFC Scanner";
            Load += Form1_Load;
            adminPanel.ResumeLayout(false);
            adminPanel.PerformLayout();
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel adminPanel;
        private Label adminID;
        private TextBox adminIDTextBox;
        private Label adminStatusLabel;
        private Panel mainPanel;
        private TextBox adminNameTextBox;
        private Button deauthenticateButton;
        private Label userID;
        private TextBox userIDTextBox;
        private Label Scanned_RFID;
        private TextBox txtRFID;
        private Label lblStatus;
        private Button btnWriteData;
        private Button btnResetUser;
        private Button btnSettings;
        private Label titleLabel;
        private PictureBox logoPictureBox;
    }
}