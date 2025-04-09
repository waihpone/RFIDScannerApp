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
        private static readonly HttpClient client = new HttpClient();

        // State variables to store scanned values
        private string? adminEmployeeNum = null;
        private string? userEmployeeNum = null;
        private string? scannedId = null;
        private bool isAdminScanned = false; // Tracks if admin has scanned

        public Form1()
        {
            InitializeComponent();
            InitializeRFIDReader();

            client.BaseAddress = new Uri("http://127.0.0.1:8000/");
            UpdateStatusLabel("Please scan the admin RFID tag to initialize the application.", Color.Black);

            this.ActiveControl = null; // OR lblStatus.Focus();
        }

        public async Task SendScannedIdAsync()
        {
            try
            {
                // Ensure all values are collected
                if (string.IsNullOrEmpty(adminEmployeeNum) || string.IsNullOrEmpty(userEmployeeNum) || string.IsNullOrEmpty(scannedId))
                {
                    Console.WriteLine("Missing data: Waiting for all scans to complete.");
                    UpdateStatusLabel("Error: Incomplete scan data.", Color.Red);
                    return;
                }

                // Prepare JSON payload
                var payload = new
                {
                    scanned_id = scannedId,
                    admin_employee_num = adminEmployeeNum,
                    user_employee_num = userEmployeeNum
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // Send POST request
                var response = await client.PostAsync("api/assets/check-by-tag", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Server response: " + responseContent);
                    UpdateStatusLabel("Processing...", Color.Black); // Temporary message
                    await Task.Delay(1000); // 1-second delay before success
                    UpdateStatusLabel("Success: Asset processed.", Color.Green); // Green for success
                    await Task.Delay(2000);
                    ResetUserAndAssetScan(); // Reset for next user/asset scan
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to send ID, status code: {response.StatusCode}");
                    Console.WriteLine("Error details: " + errorContent);
                    UpdateStatusLabel($"Error: Failed to process asset (Status: {response.StatusCode}).", Color.Red);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                UpdateStatusLabel("Error: Network issue occurred.", Color.Red);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                UpdateStatusLabel("Error: An unexpected issue occurred.", Color.Red);
            }
        }

        private void InitializeRFIDReader()
        {
            var contextFactory = ContextFactory.Instance;
            context = (SCardContext?)contextFactory.Establish(SCardScope.System);

            string[] readerNames = context.GetReaders();
            if (readerNames.Length > 0)
            {
                monitor = new SCardMonitor(contextFactory, SCardScope.System);
                monitor.CardInserted += async (sender, args) => await ReadRFIDDataAsync(args.ReaderName);
                monitor.Start(readerNames[0]);
                UpdateStatusLabel("RFID reader initialized. Waiting for admin scan...", Color.Black);
            }
            else
            {
                MessageBox.Show("No RFID readers found.");
                UpdateStatusLabel("Error: No RFID readers detected.", Color.Red);
            }
        }

        private async Task ReadRFIDDataAsync(string readerName)
        {
            Console.WriteLine($"Trying to read RFID from reader: {readerName}");

            using var context = ContextFactory.Instance.Establish(SCardScope.System);
            using var reader = new SCardReader(context);

            // Step 1: Connect to the reader
            SCardError connectResult = reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
            if (connectResult != SCardError.Success)
            {
                Console.WriteLine($"Error: Unable to connect to RFID reader. Error Code: {connectResult}");
                UpdateStatusLabel($"Error: Unable to connect to RFID reader ({connectResult}).", Color.Red);
                return;
            }

            // Step 2: Detect card type
            string cardType = await DetectCardTypeAsync(reader);
            Console.WriteLine($"Detected card type: {cardType}");

            // Step 3: Read based on detected card type
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
                    UpdateStatusLabel("Error: Unsupported RFID card type.", Color.Red);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading card: {ex.Message}");
                UpdateStatusLabel("Error: Failed to read RFID tag.", Color.Red);
                return;
            }

            // Step 4: Send RFID data
            if (!string.IsNullOrEmpty(rfidData))
            {
                Console.WriteLine($"RFID Read Success: {rfidData}");

                if (!isAdminScanned)
                {
                    // First scan: Admin
                    adminEmployeeNum = rfidData;
                    isAdminScanned = true;
                    UpdateUI("admin", rfidData);
                    UpdateStatusLabel("Admin scan successful. Please scan the user RFID tag.", Color.Green);
                }
                else if (string.IsNullOrEmpty(userEmployeeNum))
                {
                    // Second scan: User
                    userEmployeeNum = rfidData;
                    UpdateUI("user", rfidData);
                    UpdateStatusLabel("User scan successful. Please scan the asset RFID tag.", Color.Green);
                }
                else
                {
                    // Third scan: Asset
                    scannedId = rfidData;
                    UpdateUI("asset", rfidData);
                    UpdateStatusLabel("Asset scanned. Sending data to server...", Color.Green);
                    await Task.Delay(1000);
                    await SendScannedIdAsync();
                }
            }
            else
            {
                Console.WriteLine("Error: Unable to read RFID tag.");
                UpdateStatusLabel("Error: Unable to read RFID tag. Please try again.", Color.Red);
            }
        }

        private void ResetUserAndAssetScan()
        {
            if (IsDisposed)
            {
                Console.WriteLine("Form is disposed; skipping reset.");
                return;
            }

            try
            {
                Console.WriteLine("Resetting user and asset fields...");
                userEmployeeNum = null;
                scannedId = null;

                if (InvokeRequired)
                {
                    Console.WriteLine("Invoke required; delegating UI updates to main thread...");
                    Invoke(new Action(() =>
                    {
                        ResetUITextBoxes();
                        UpdateStatusLabel("Scan completed. Please scan the next user RFID tag.", Color.Green);
                    }));
                }
                else
                {
                    ResetUITextBoxes();
                    UpdateStatusLabel("Scan completed. Please scan the next user RFID tag.", Color.Green);
                }

                Console.WriteLine("Reset completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ResetUserAndAssetScan: {ex.Message}\nStackTrace: {ex.StackTrace}");
                UpdateStatusLabel("Error: Failed to reset UI.", Color.Red);
            }
        }

        private void ResetUITextBoxes()
        {
            try
            {
                if (userIDTextBox == null || userIDTextBox.IsDisposed)
                    Console.WriteLine("Warning: userIDTextBox is null or disposed.");
                else
                {
                    userIDTextBox.Text = string.Empty;
                    Console.WriteLine("userIDTextBox reset.");
                }

                if (txtRFID == null || txtRFID.IsDisposed)
                    Console.WriteLine("Warning: txtRFID is null or disposed.");
                else
                {
                    txtRFID.Text = string.Empty;
                    Console.WriteLine("txtRFID reset.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ResetUITextBoxes: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw; // Re-throw to be caught by ResetUserAndAssetScan
            }
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

        private void UpdateStatusLabel(string message, Color color)
        {
            if (lblStatus == null || lblStatus.IsDisposed)
            {
                Console.WriteLine("Warning: lblStatus is null or disposed.");
                return;
            }

            if (InvokeRequired)
            {
                try
                {
                    lblStatus.Invoke(new Action(() =>
                    {
                        lblStatus.Text = message;
                        lblStatus.ForeColor = color;
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
                    lblStatus.Text = message;
                    lblStatus.ForeColor = color;
                    Console.WriteLine($"Status label updated: {message} in {color.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in UpdateStatusLabel: {ex.Message}\nStackTrace: {ex.StackTrace}");
                }
            }
        }

        private async Task<string?> ReadMifareClassicNDEFAsync(SCardReader reader)
        {
            byte[] keyA = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            List<byte> rawData = new List<byte>();

            for (int sector = 1; sector < 16; sector++)  // Start from sector 1
            {
                byte sectorStartBlock = (byte)(sector * 4);

                // Authenticate at the first block of the sector
                byte[] authCommand = { 0xFF, 0x86, 0x00, 0x00, 0x05, 0x01, 0x00, sectorStartBlock, 0x60, 0x00 };
                byte[] authResponse = new byte[2];
                int authResponseLength = authResponse.Length;

                SCardError authResult = reader.Transmit(authCommand, authCommand.Length, authResponse, ref authResponseLength);
                if (authResult != SCardError.Success || authResponseLength == 0 || authResponse[0] != 0x90)
                {
                    //MessageBox.Show($"Authentication failed at Sector {sector} (Block {sectorStartBlock})");
                    continue;  // Skip sector if authentication fails
                }

                for (int block = sectorStartBlock; block < (sectorStartBlock + 3); block++)  // Read 3 blocks (excluding trailer)
                {
                    byte[] readCommand = { 0xFF, 0xB0, 0x00, (byte)block, 0x10 }; // 16 bytes per block
                    byte[] dataBuffer = new byte[64];  // Larger buffer for safety
                    int recvLength = dataBuffer.Length;

                    SCardError readResult = reader.Transmit(readCommand, readCommand.Length, dataBuffer, ref recvLength);
                    if (readResult == SCardError.Success && recvLength > 0)
                    {
                        rawData.AddRange(dataBuffer.Take(16));  // Read exactly 16 bytes per block

                        // Stop reading if NDEF terminator (0xFE) is found
                        if (dataBuffer.Contains((byte)0xFE))
                            break;
                    }
                    else
                    {
                        //MessageBox.Show($"Read Error at Block {block}: {readResult}");
                        continue;  // Skip this block and continue
                    }
                }
            }

            byte[] ndefBytes = rawData.ToArray();
            string hexData = BitConverter.ToString(ndefBytes).Replace("-", " ");
            Console.WriteLine("HEX Data: " + hexData);  // Debug output

            // Parse NDEF Text Record
            string? parsedData = NDEFParser(ndefBytes);
            return parsedData;
        }

        private async Task<string?> ReadNtag213DataAsync(SCardReader reader)
        {
            List<byte> rawData = new List<byte>();
            int startPage = 4; // NTAG213 user data starts from page 4
            int totalPages = 36; // NTAG213 has 144 bytes of user memory (36 pages)

            for (int page = startPage; page < startPage + totalPages; page += 4) // Read in 4-page (16-byte) blocks
            {
                byte[] readCommand = { 0xFF, 0xB0, 0x00, (byte)page, 0x10 }; // Read 16 bytes
                byte[] dataBuffer = new byte[64]; // Larger buffer for safety
                int recvLength = dataBuffer.Length;

                SCardError readResult = reader.Transmit(readCommand, readCommand.Length, dataBuffer, ref recvLength);

                if (readResult == SCardError.Success && recvLength > 0)
                {
                    rawData.AddRange(dataBuffer.Take(16)); // Read exactly 16 bytes per block

                    // Stop reading if NDEF terminator (0xFE) is found
                    if (dataBuffer.Contains((byte)0xFE))
                        break;
                }
                else
                {
                    //MessageBox.Show($"Read Error at Page {page}: {readResult}");
                    continue; // Skip this block and continue
                }
            }

            byte[] ndefBytes = rawData.ToArray();
            string hexData = BitConverter.ToString(ndefBytes).Replace("-", " ");
            Console.WriteLine("HEX Data: " + hexData); // Debug output

            // Parse NDEF Text Record
            string? parsedData = NDEFParser(ndefBytes);
            return parsedData;
        }

        private string? NDEFParser(byte[] ndefBytes)
        {
            if (ndefBytes.Length < 3)
                return null;

            Console.WriteLine("Raw Data (Hex): " + BitConverter.ToString(ndefBytes));

            // Find "en" dynamically
            int index = -1;
            for (int i = 0; i < ndefBytes.Length - 1; i++)
            {
                if (ndefBytes[i] == 0x65 && ndefBytes[i + 1] == 0x6E) // "en" in ASCII
                {
                    index = i;
                    break;
                }
            }

            if (index == -1 || index + 2 >= ndefBytes.Length) // If "en" not found or no data after
                return null;

            index += 2; // Move past "en"

            // Extract data
            byte[] extractedData = ndefBytes.Skip(index).ToArray();

            // **Remove trailing null or invalid characters**
            extractedData = extractedData.TakeWhile(b => b != 0x00 && b != 0xFE).ToArray();

            string textPayload = Encoding.UTF8.GetString(extractedData);

            Console.WriteLine("Extracted Payload (UTF-8): " + textPayload);
            return textPayload.Trim(); // Trim spaces or stray characters
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            monitor?.Cancel();
            monitor?.Dispose();
            context?.Dispose();
            client?.Dispose(); // Clean up HttpClient
        }
    }
}
