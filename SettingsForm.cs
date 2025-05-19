using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RFIDScannerApp
{
    public partial class SettingsForm : Form
    {
        public string ServerUri { get; private set; }

        public SettingsForm(string currentUri = "")
        {
            InitializeComponent();

            txtUri.Text = currentUri;
            btnSave.Enabled = IsValidUri(txtUri.Text);
        }

        private void TxtUri_TextChanged(object sender, EventArgs e)
        {
            btnSave.Enabled = IsValidUri(txtUri.Text);
        }

        private bool IsValidUri(string uri)
        {
            return Uri.TryCreate(uri.Trim(), UriKind.Absolute, out Uri result)
                   && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            ServerUri = txtUri.Text.Trim();

            // Save to settings
            Properties.Settings.Default.ServerUri = ServerUri;
            Properties.Settings.Default.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnSave_EnabledChanged(object sender, EventArgs e)
        {
            if (btnSave.Enabled)
            {
                btnSave.FlatAppearance.BorderColor = UIHelpers.HexToColor("#027f80");
                btnSave.FlatAppearance.BorderSize = 1;
                btnSave.BackColor = UIHelpers.HexToColor("#141414");
                btnSave.ForeColor = Color.White;
            }
            else
            {
                btnSave.FlatAppearance.BorderColor = UIHelpers.HexToColor("#555555"); // Muted gray border
                btnSave.FlatAppearance.BorderSize = 1;
                btnSave.BackColor = UIHelpers.HexToColor("#3a3a3a"); // Dimmed background
                btnSave.ForeColor = UIHelpers.HexToColor("#aaaaaa"); // Light gray text
            }
        }


    }
}
