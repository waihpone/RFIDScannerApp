using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using PCSC;
using PCSC.Monitoring;

namespace RFIDScannerApp
{
    public partial class Form1 : Form
    {
        private SCardMonitor? monitor;
        private SCardContext? context;
        private HttpClient client;
        private string? adminEmployeeNum;
        private string? userEmployeeNum;
        private string? scannedId;
        private string serverUri;
        private string currentReaderName;

        public Form1()
        {
            InitializeComponent();
            client = new HttpClient();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeRFIDReader();

            try
            {
                serverUri = Properties.Settings.Default.ServerUri;
                if (!string.IsNullOrWhiteSpace(serverUri))
                {
                    client.BaseAddress = new Uri(serverUri);
                }
            }
            catch (UriFormatException)
            {
                MessageBox.Show("Saved server URI is invalid. Please reconfigure it in Settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            UpdateStatusLabel("Please scan the admin RFID tag to initialize the application.", UIHelpers.HexToColor("#027f80"), true);
            this.ActiveControl = null;
        }

        private void InitializeRFIDReader()
        {
            var contextFactory = ContextFactory.Instance;

            try
            {
                context = (SCardContext?)contextFactory.Establish(SCardScope.System);

                string[] readerNames = context.GetReaders();
                if (readerNames.Length == 0)
                {
                    MessageBox.Show("No RFID readers found.");
                    UpdateStatusLabel("Error: No RFID readers detected.", Color.Red, true);
                    return;
                }

                if (monitor != null)
                {
                    monitor.Cancel();
                    monitor.Dispose();
                    monitor = null;
                }

                currentReaderName = readerNames[0];
                monitor = new SCardMonitor(contextFactory, SCardScope.System);
                monitor.CardInserted += async (sender, args) => await ReadRFIDDataAsync(args.ReaderName);
                monitor.CardRemoved += (sender, args) => UpdateStatusLabel("Card removed.", Color.Gray, adminPanel.Visible);
                monitor.MonitorException += (sender, ex) => UpdateStatusLabel("Monitor error: " + ex.Message, Color.Red, adminPanel.Visible);

                monitor.Start(currentReaderName);
                UpdateStatusLabel("RFID reader initialized. Waiting for admin scan...", UIHelpers.HexToColor("#027f80"), true);
            }
            catch (Exception ex)
            {
                UpdateStatusLabel("Failed to initialize RFID reader: " + ex.Message, Color.Red, true);
            }
        }

        private async Task ReadRFIDDataAsync(string readerName)
        {
            Console.WriteLine($"Trying to read RFID from reader: {readerName}");

            using var context = ContextFactory.Instance.Establish(SCardScope.System);
            using var reader = new SCardReader(context);

            SCardError connectResult = reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
            if (connectResult != SCardError.Success)
            {
                Console.WriteLine($"Error: Unable to connect to RFID reader. Error Code: {connectResult}");
                UpdateStatusLabel($"Error: Unable to connect to RFID reader ({connectResult}).", Color.Red, adminPanel.Visible);
                return;
            }

            string cardType = await DetectCardTypeAsync(reader);
            Console.WriteLine($"Detected card type: {cardType}");

            string? rfidData = null;
            try
            {
                if (cardType == "MIFARE_CLASSIC")
                {
                    Console.WriteLine("Attempting MIFARE Classic read...");
                    rfidData = await ReadMifareClassicNDEFAsync(reader);
                }
                else if (cardType == "NTAG213")
                {
                    Console.WriteLine("Attempting NTAG213 read...");
                    rfidData = await ReadNtag213DataAsync(reader);
                }
                else
                {
                    Console.WriteLine("Unsupported card type.");
                    UpdateStatusLabel("Error: Unsupported RFID card type.", Color.Red, adminPanel.Visible);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading card: {ex.Message}");
                UpdateStatusLabel("Error: Failed to read RFID tag.", Color.Red, adminPanel.Visible);
                return;
            }

            if (!string.IsNullOrEmpty(rfidData))
            {
                Console.WriteLine($"RFID Read Success: {rfidData}");

                if (adminPanel.Visible)
                {
                    // Validate Admin ID
                    if (!ValidateAdminOrUserID(rfidData, "Admin"))
                    {
                        adminEmployeeNum = null;
                        UpdateUI("admin", "");
                        return;
                    }

                    adminEmployeeNum = rfidData;
                    UpdateUI("admin", rfidData);

                    var (isAdmin, firstName, lastName) = await ValidateEmployeeID(rfidData);

                    if (isAdmin)
                    {
                        this.Invoke(new Action(() =>
                        {
                            UpdateStatusLabel($"Admin: {firstName} {lastName} verified", Color.Green, true);
                            adminIDTextBox.Text = $"{firstName} {lastName}";
                        }));

                        await Task.Delay(2000);
                        SwitchToMainPanel(firstName, lastName);
                    }
                    else
                    {
                        UpdateStatusLabel("Invalid admin ID. Try again.", Color.Red, true);
                        adminEmployeeNum = null;
                        UpdateUI("admin", "");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(userEmployeeNum))
                    {
                        // Validate User ID
                        if (!ValidateAdminOrUserID(rfidData, "User"))
                        {
                            userEmployeeNum = null;
                            UpdateUI("user", "");
                            return;
                        }

                        userEmployeeNum = rfidData;
                        UpdateUI("user", rfidData);

                        var (isAdmin, firstName, lastName) = await ValidateEmployeeID(rfidData);

                        this.Invoke(new Action(() =>
                        {
                            userIDTextBox.Text = $"{firstName} {lastName}";
                        }));

                        await Task.Delay(2000);

                        UpdateStatusLabel("User scan successful. Please scan the asset RFID tag.", Color.Green, false);
                    }
                    else
                    {
                        // Validate Asset ID
                        if (!ValidateAssetID(rfidData))
                        {
                            scannedId = null;
                            UpdateUI("asset", "");
                            return;
                        }

                        scannedId = rfidData;
                        UpdateUI("asset", rfidData);
                        UpdateStatusLabel("Asset scanned. Sending data to server...", Color.Green, false);
                        await Task.Delay(1000);
                        await SendScannedIdAsync();
                    }
                }
            }
            else
            {
                Console.WriteLine("Error: Unable to read RFID tag.");
                UpdateStatusLabel("Error: Unable to read RFID tag. Please try again.", Color.Red, adminPanel.Visible);
            }
        }

        private async Task<(bool isAdmin, string firstName, string lastName)> ValidateEmployeeID(string employeeId)
        {
            try
            {
                // Send GET request to the new endpoint
                var response = await client.GetAsync($"/api/users/{employeeId}/roles");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to validate admin ID. Status code: {response.StatusCode}");
                    UpdateStatusLabel("Error: Could not reach server.", Color.Red, true);
                    return (false, "", "");
                }

                // Parse JSON response
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                // Check for error response
                if (data.ContainsKey("error"))
                {
                    Console.WriteLine($"Error from server: {data["error"]}");
                    UpdateStatusLabel($"Error: {data["error"]}", Color.Red, true);
                    return (false, "", "");
                }

                if (!data.ContainsKey("roles"))
                {
                    Console.WriteLine("No roles found in response.");
                    UpdateStatusLabel("Error: No roles found for user.", Color.Red, true);
                    return (false, "", "");
                }

                // Extract roles and check for Admin or Superadmin
                var roles = JsonSerializer.Deserialize<List<string>>(data["roles"].ToString());
                bool isAdmin = roles.Contains("Admin") || roles.Contains("SuperAdmin");

                string firstName = data.ContainsKey("first_name") ? data["first_name"].ToString() : null;
                string lastName = data.ContainsKey("last_name") ? data["last_name"].ToString() : null;

                Console.WriteLine($"Employee ID {employeeId} has roles: {string.Join(", ", roles)}. IsAdmin: {isAdmin}");
                return (isAdmin, firstName, lastName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating employee ID: {ex.Message}");
                UpdateStatusLabel("Error: Failed to validate admin ID.", Color.Red, true);
                return (false, "", "");
            }
        }

        private void SwitchToMainPanel(string firstName, string lastName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SwitchToMainPanel(firstName, lastName)));
                return;
            }
            adminPanel.Visible = false;
            mainPanel.Visible = true;
            adminNameTextBox.Text = $"{firstName} {lastName}";
            lblStatus.Text = "Waiting for user scan...";
            lblStatus.ForeColor = UIHelpers.HexToColor("#027f80");
        }

        private void ResetToAdminPanel()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ResetToAdminPanel));
                return;
            }
            mainPanel.Visible = false;
            adminPanel.Visible = true;
            adminEmployeeNum = null;
            userEmployeeNum = null;
            scannedId = null;
            adminIDTextBox.Text = "";
            userIDTextBox.Text = "";
            txtRFID.Text = "";
            adminNameTextBox.Text = "";
            adminStatusLabel.Text = "Waiting for admin scan...";
            adminStatusLabel.ForeColor = UIHelpers.HexToColor("#027f80");
            lblStatus.Text = "Waiting for scan...";
            lblStatus.ForeColor = UIHelpers.HexToColor("#027f80");
        }

        private void UpdateUI(string type, string rfidData)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUI(type, rfidData)));
                return;
            }

            switch (type.ToLower())
            {
                case "admin":
                    adminIDTextBox.Text = rfidData;
                    break;
                case "user":
                    userIDTextBox.Text = rfidData;
                    break;
                case "asset":
                    txtRFID.Text = rfidData;
                    break;
                default:
                    Console.WriteLine($"Unknown UI update type: {type}");
                    break;
            }
        }

        private void UpdateStatusLabel(string message, Color color, bool isAdminPanel)
        {
            Label targetLabel = isAdminPanel ? adminStatusLabel : lblStatus;

            if (targetLabel == null || targetLabel.IsDisposed)
            {
                Console.WriteLine($"Warning: {targetLabel?.Name ?? "Status label"} is null or disposed.");
                return;
            }

            if (InvokeRequired)
            {
                try
                {
                    targetLabel.Invoke(new Action(() =>
                    {
                        targetLabel.Text = message;
                        targetLabel.ForeColor = color;
                    }));
                    Console.WriteLine($"Status label updated (Invoke): {message} in {color.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in UpdateStatusLabel (Invoke): {ex.Message}\nStackTrace: {ex.StackTrace}");
                }
            }
            else
            {
                try
                {
                    targetLabel.Text = message;
                    targetLabel.ForeColor = color;
                    Console.WriteLine($"Status label updated: {message} in {color.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in UpdateStatusLabel: {ex.Message}\nStackTrace: {ex.StackTrace}");
                }
            }
        }

        private async Task SendScannedIdAsync()
        {
            string resetMessage = "Unknown error occurred during processing.";
            Color resetColor = Color.Red;

            try
            {
                // Only require adminEmployeeNum and userEmployeeNum for the first scan
                if (string.IsNullOrEmpty(adminEmployeeNum) || (string.IsNullOrEmpty(userEmployeeNum) && string.IsNullOrEmpty(scannedId)))
                {
                    Console.WriteLine("Missing data: Waiting for all scans to complete.");
                    UpdateStatusLabel("Error: Incomplete scan data.", Color.Red, false);
                    return;
                }

                // If userEmployeeNum is set, we only need scannedId to proceed
                if (string.IsNullOrEmpty(scannedId))
                {
                    Console.WriteLine("No asset scanned.");
                    UpdateStatusLabel("Error: No asset scanned.", Color.Red, false);
                    return;
                }

                var payload = new
                {
                    scanned_id = scannedId,
                    admin_employee_num = adminEmployeeNum,
                    user_employee_num = userEmployeeNum
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/assets/check-by-tag", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Server response: " + responseContent);
                    UpdateStatusLabel("Processing...", UIHelpers.HexToColor("#027f80"), false);
                    await Task.Delay(1000);
                    UpdateStatusLabel("Success: Asset processed.", Color.Green, false);
                    resetMessage = "Asset processed. Ready for next asset scan.";
                    resetColor = Color.Green;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to send ID, status code: {response.StatusCode}");
                    Console.WriteLine("Error details: " + errorContent);
                    UpdateStatusLabel($"Error: Failed to process asset (Status: {response.StatusCode}).", Color.Red, false);
                    resetMessage = "Error processing asset. Ready for next asset scan.";
                    resetColor = UIHelpers.HexToColor("#027f80");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                UpdateStatusLabel("Error: Network issue occurred.", Color.Red, false);
                resetMessage = "Error processing asset. Ready for next asset scan.";
                resetColor = UIHelpers.HexToColor("#027f80");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                UpdateStatusLabel("Error: Invalid server response.", Color.Red, false);
                resetMessage = "Error processing asset. Ready for next asset scan.";
                resetColor = UIHelpers.HexToColor("#027f80");
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"Request timeout: {ex.Message}\nStackTrace: {ex.StackTrace}");
                UpdateStatusLabel("Error: Server request timed out.", Color.Red, false);
                resetMessage = "Error processing asset. Ready for next asset scan.";
                resetColor = UIHelpers.HexToColor("#027f80");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                UpdateStatusLabel("Error: An unexpected issue occurred.", Color.Red, false);
                resetMessage = "Error processing asset. Ready for next asset scan.";
                resetColor = UIHelpers.HexToColor("#027f80");
            }
            finally
            {
                try
                {
                    await Task.Delay(2000);
                    // Only reset the asset-related fields
                    scannedId = null;

                    // Update UI on UI thread
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            if (!txtRFID.IsDisposed) txtRFID.Text = "";
                            UpdateStatusLabel("Waiting for asset scan...", UIHelpers.HexToColor("#027f80"), false);
                        }));
                    }
                    else
                    {
                        if (!txtRFID.IsDisposed) txtRFID.Text = "";
                        UpdateStatusLabel("Waiting for asset scan...", UIHelpers.HexToColor("#027f80"), false);
                    }

                    Console.WriteLine("Reset for new asset scan. userEmployeeNum={0}, scannedId={1}", userEmployeeNum, scannedId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in finally block: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    UpdateStatusLabel("Error: Failed to reset for new scan.", Color.Red, false);
                }
            }
        }

        private void BtnResetUser_Click(object sender, EventArgs e)
        {
            // Reset user-related fields
            userEmployeeNum = null;

            // Reset asset-related fields
            scannedId = null;

            // Update UI on the UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    if (!userIDTextBox.IsDisposed) userIDTextBox.Text = "";
                    if (!txtRFID.IsDisposed) txtRFID.Text = "";
                    UpdateStatusLabel("Waiting for user scan...", UIHelpers.HexToColor("#027f80"), false);
                }));
            }
            else
            {
                if (!userIDTextBox.IsDisposed) userIDTextBox.Text = "";
                if (!txtRFID.IsDisposed) txtRFID.Text = "";
                UpdateStatusLabel("Waiting for user scan...", UIHelpers.HexToColor("#027f80"), false);
            }

            Console.WriteLine("User reset. userEmployeeNum={0}, scannedId={1}", userEmployeeNum, scannedId);
        }

        private async Task<string?> ReadMifareClassicNDEFAsync(SCardReader reader)
        {
            byte[] keyA = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            List<byte> rawData = new List<byte>();

            for (int sector = 1; sector < 16; sector++)
            {
                byte sectorStartBlock = (byte)(sector * 4);
                byte[] authCommand = { 0xFF, 0x86, 0x00, 0x00, 0x05, 0x01, 0x00, sectorStartBlock, 0x60, 0x00 };
                byte[] authResponse = new byte[2];
                int authResponseLength = authResponse.Length;

                SCardError authResult = reader.Transmit(authCommand, authCommand.Length, authResponse, ref authResponseLength);
                if (authResult != SCardError.Success || authResponseLength == 0 || authResponse[0] != 0x90)
                {
                    continue;
                }

                for (int block = sectorStartBlock; block < (sectorStartBlock + 3); block++)
                {
                    byte[] readCommand = { 0xFF, 0xB0, 0x00, (byte)block, 0x10 };
                    byte[] dataBuffer = new byte[64];
                    int recvLength = dataBuffer.Length;

                    SCardError readResult = reader.Transmit(readCommand, readCommand.Length, dataBuffer, ref recvLength);
                    if (readResult == SCardError.Success && recvLength > 0)
                    {
                        rawData.AddRange(dataBuffer.Take(16));
                        if (dataBuffer.Contains((byte)0xFE))
                            break;
                    }
                }
            }

            byte[] ndefBytes = rawData.ToArray();
            string hexData = BitConverter.ToString(ndefBytes).Replace("-", " ");
            Console.WriteLine("HEX Data: " + hexData);

            return NDEFParser(ndefBytes);
        }

        private async Task<string?> ReadNtag213DataAsync(SCardReader reader)
        {
            List<byte> rawData = new List<byte>();
            int startPage = 4;
            int totalPages = 36;

            for (int page = startPage; page < startPage + totalPages; page += 4)
            {
                byte[] readCommand = { 0xFF, 0xB0, 0x00, (byte)page, 0x10 };
                byte[] dataBuffer = new byte[64];
                int recvLength = dataBuffer.Length;

                SCardError readResult = reader.Transmit(readCommand, readCommand.Length, dataBuffer, ref recvLength);
                if (readResult == SCardError.Success && recvLength > 0)
                {
                    rawData.AddRange(dataBuffer.Take(16));
                    if (dataBuffer.Contains((byte)0xFE))
                        break;
                }
            }

            byte[] ndefBytes = rawData.ToArray();
            string hexData = BitConverter.ToString(ndefBytes).Replace("-", " ");
            Console.WriteLine("HEX Data: " + hexData);

            return NDEFParser(ndefBytes);
        }

        private string? NDEFParser(byte[] ndefBytes)
        {
            if (ndefBytes.Length < 3)
                return null;

            Console.WriteLine("Raw Data (Hex): " + BitConverter.ToString(ndefBytes));

            int index = -1;
            for (int i = 0; i < ndefBytes.Length - 1; i++)
            {
                if (ndefBytes[i] == 0x65 && ndefBytes[i + 1] == 0x6E)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1 || index + 2 >= ndefBytes.Length)
                return null;

            index += 2;
            byte[] extractedData = ndefBytes.Skip(index).TakeWhile(b => b != 0x00 && b != 0xFE).ToArray();
            string textPayload = Encoding.UTF8.GetString(extractedData);

            Console.WriteLine("Extracted Payload (UTF-8): " + textPayload);
            return textPayload.Trim();
        }

        private void PauseScanning()
        {
            if (monitor != null)
            {
                monitor.Cancel();
                monitor.Dispose();
                monitor = null;
                UpdateStatusLabel("Scanning paused.", Color.Orange, adminPanel.Visible);
            }
        }

        private void ResumeScanning()
        {
            InitializeRFIDReader();
            UpdateStatusLabel("Scanning resumed.", Color.Green, adminPanel.Visible);
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            PauseScanning();
            var settingsForm = new SettingsForm(serverUri);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                serverUri = settingsForm.ServerUri;
                // Dispose old client and create new one
                client?.Dispose();
                client = new HttpClient();
                try
                {
                    client.BaseAddress = new Uri(serverUri);
                    UpdateStatusLabel("Server URI updated: " + serverUri, Color.Green, adminPanel.Visible);
                }
                catch (UriFormatException)
                {
                    MessageBox.Show("Invalid server URI entered. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    UpdateStatusLabel("Error: Invalid server URI.", Color.Red, adminPanel.Visible);
                }
            }
            ResumeScanning();
        }

        private async void BtnWriteData_Click(object sender, EventArgs e)
        {
            PauseScanning();
            var writeDataForm = new WriteForm(currentReaderName);
            writeDataForm.ShowDialog();
            ResumeScanning();
        }

        private void deauthenticateButton_Click(object sender, EventArgs e)
        {
            ResetToAdminPanel();
            UpdateStatusLabel("Admin deauthenticated. Please scan admin RFID tag.", UIHelpers.HexToColor("#027f80"), true);
        }

        private async Task<string> DetectCardTypeAsync(SCardReader reader)
        {
            byte[] getUIDCommand = new byte[] { 0xFF, 0xCA, 0x00, 0x00, 0x00 }; // APDU command to get UID
            byte[] response = new byte[256];
            int responseLength = response.Length;

            SCardError sendResult = reader.Transmit(getUIDCommand, getUIDCommand.Length, response, ref responseLength);

            if (sendResult == SCardError.Success && responseLength > 2) // Ensure response is valid
            {
                byte[] uidBytes = response.Take(responseLength - 2).ToArray(); // Remove last 2 bytes (SW1 SW2)
                string uidHex = BitConverter.ToString(uidBytes);

                Console.WriteLine($"Card UID (Raw): {uidHex}");

                if (uidBytes.Length == 4) return "MIFARE_CLASSIC";
                if (uidBytes.Length == 7) return "NTAG213";
            }
            else
            {
                Console.WriteLine($"APDU command failed: {sendResult}");
            }

            return "UNKNOWN";
        }

        private bool ValidateAdminOrUserID(string id, string type)
        {
            // Check if the ID is exactly 8 digits
            if (id.Length != 8 || !long.TryParse(id, out long idNumber))
            {
                UpdateStatusLabel($"Error: {type} ID format is incorrect.", Color.Red, adminPanel.Visible);
                Console.WriteLine($"{type} ID validation failed: {id} is not an 8-digit number.");
                return false;
            }

            // Check if the ID is within the range 10000000 to 99999999
            if (idNumber < 10000000 || idNumber > 99999999)
            {
                UpdateStatusLabel($"Error: {type} ID must be between 10000000 and 99999999.", Color.Red, adminPanel.Visible);
                Console.WriteLine($"{type} ID validation failed: {id} is out of range (10000000-99999999).");
                return false;
            }

            return true;
        }

        private bool ValidateAssetID(string assetId)
        {
            if (!IsValidUUID(assetId))
            {
                UpdateStatusLabel("Error: Asset ID must be a valid UUID.", Color.Red, false);
                Console.WriteLine($"Asset ID validation failed: {assetId} is not a valid UUID.");
                return false;
            }

            return true;
        }

        private bool IsValidUUID(string uuid)
        {
            // UUID format: 8-4-4-4-12 (e.g., 123e4567-e89b-12d3-a456-426614174000)
            string pattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
            return System.Text.RegularExpressions.Regex.IsMatch(uuid, pattern);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            monitor?.Cancel();
            monitor?.Dispose();
            context?.Dispose();
            client?.Dispose();
        }
    }
}