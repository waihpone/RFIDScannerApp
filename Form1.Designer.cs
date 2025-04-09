namespace RFIDScannerApp
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
            this.Text = "RFID/NFC Scanner";
            this.Size = new Size(550, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = Color.Black; // Light theme

            // === PANEL (Like a modern card UI) ===
            Panel mainPanel = new Panel();
            mainPanel.Size = new Size(450, 250);
            mainPanel.Location = new Point(50, 30);
            mainPanel.BackColor = Color.White;
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.Padding = new Padding(10);

            // === TITLE LABEL ===
            Label titleLabel = new Label();
            titleLabel.Text = "RFID/NFC Scanner";
            titleLabel.Font = new Font("Arial", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(120, 10);

            // === Admin ID Label ===
            adminID = new Label();
            adminID.Text = "ADMIN ID";
            adminID.Font = new Font("Arial", 10, FontStyle.Bold);
            adminID.ForeColor = Color.Black;
            adminID.Location = new Point(20, 50);

            // === Admin ID TextBox (Rounded) ===
            adminIDTextBox = new TextBox();
            adminIDTextBox.Size = new Size(300, 27);
            adminIDTextBox.Location = new Point(120, 45);
            adminIDTextBox.ReadOnly = true;
            adminIDTextBox.BackColor = Color.White;
            adminIDTextBox.BorderStyle = BorderStyle.FixedSingle;

            // === User ID Label ===
            userID = new Label();
            userID.Text = "USER ID";
            userID.Font = new Font("Arial", 10, FontStyle.Bold);
            userID.ForeColor = Color.Black;
            userID.Location = new Point(20, 90);

            // === User ID TextBox (Rounded) ===
            userIDTextBox = new TextBox();
            userIDTextBox.Size = new Size(300, 27);
            userIDTextBox.Location = new Point(120, 85);
            userIDTextBox.ReadOnly = true;
            userIDTextBox.BackColor = Color.White;
            userIDTextBox.BorderStyle = BorderStyle.FixedSingle;

            // === RFID Label ===
            Scanned_RFID = new Label();
            Scanned_RFID.Text = "ASSET ID";
            Scanned_RFID.Font = new Font("Arial", 10, FontStyle.Bold);
            Scanned_RFID.ForeColor = Color.Black;
            Scanned_RFID.Location = new Point(20, 130);

            // === RFID TextBox (Rounded) ===
            txtRFID = new TextBox();
            txtRFID.Size = new Size(300, 27);
            txtRFID.Location = new Point(120, 125);
            txtRFID.ReadOnly = true;
            txtRFID.BackColor = Color.White;
            txtRFID.BorderStyle = BorderStyle.FixedSingle;

            // === STATUS LABEL ===
            lblStatus = new Label();
            lblStatus.Text = "Waiting for scan...";
            lblStatus.Font = new Font("Arial", 9, FontStyle.Italic);
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(120, 165);
            lblStatus.Size = new Size(250, 30);

            // === ADD ELEMENTS TO PANEL ===
            mainPanel.Controls.Add(adminID);
            mainPanel.Controls.Add(adminIDTextBox);
            mainPanel.Controls.Add(userID);
            mainPanel.Controls.Add(userIDTextBox);
            mainPanel.Controls.Add(Scanned_RFID);
            mainPanel.Controls.Add(txtRFID);
            mainPanel.Controls.Add(lblStatus);

            // === ADD ELEMENTS TO FORM ===
            this.Controls.Add(titleLabel);
            this.Controls.Add(mainPanel);
        }


        #endregion

        private TextBox txtRFID;
        private Label lblStatus;
        private Label Scanned_RFID;
        private Label adminID;
        private Label userID;
        private TextBox adminIDTextBox;
        private TextBox userIDTextBox;
    }
}
