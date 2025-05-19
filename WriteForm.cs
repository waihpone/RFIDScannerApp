using PCSC;
using PCSC.Monitoring;
using PCSC.Utils;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RFIDScannerApp
{
    public partial class WriteForm : Form
    {
        private readonly SCardContext _context;
        private readonly string _readerName;
        private SCardMonitor _monitor;

        public WriteForm(string readerName)
        {
            InitializeComponent();
            _readerName = readerName;
            _context = new SCardContext();
            _context.Establish(SCardScope.System);
            InitializeCardMonitor();
        }

        private void InitializeCardMonitor()
        {
            try
            {
                var contextFactory = ContextFactory.Instance;
                _monitor = new SCardMonitor(contextFactory, SCardScope.System);
                _monitor.CardInserted += async (sender, args) => await OnCardInsertedAsync(args.ReaderName);
                _monitor.CardRemoved += (sender, args) => UpdateStatusLabel("Ready to write...", UIHelpers.HexToColor("#027f80"));
                _monitor.MonitorException += (sender, ex) => UpdateStatusLabel($"Monitor error: {ex.Message}", Color.Red);
                _monitor.Start(_readerName);
                UpdateStatusLabel("Ready to write...", UIHelpers.HexToColor("#027f80"));
            }
            catch (Exception ex)
            {
                UpdateStatusLabel($"Failed to initialize card monitor: {ex.Message}", Color.Red);
            }
        }

        private async Task OnCardInsertedAsync(string readerName)
        {
            try
            {
                using (var reader = new SCardReader(_context))
                {
                    var connection = reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                    if (connection != SCardError.Success)
                    {
                        UpdateStatusLabel($"Reader error: {SCardHelper.StringifyError(connection)}", Color.Red);
                        return;
                    }

                    string cardType = await DetectCardTypeAsync(reader);
                    Console.WriteLine($"Detected Card Type: {cardType}");

                    if (cardType == "MIFARE_CLASSIC")
                    {
                        UpdateStatusLabel("RFID card detected. Please enter the desired user ID in the correct format and write the data.", Color.Green);
                    }
                    else if (cardType == "NTAG213")
                    {
                        UpdateStatusLabel("NFC tag detected. Please enter the desired asset ID in the correct format and write the data.", Color.Green);
                    }
                    else
                    {
                        UpdateStatusLabel("Unsupported or unknown card type.", Color.Red);
                    }

                    reader.Disconnect(SCardReaderDisposition.Leave);
                }
            }
            catch (Exception ex)
            {
                UpdateStatusLabel($"Error detecting card: {ex.Message}", Color.Red);
            }
        }

        private void UpdateStatusLabel(string message, Color color)
        {
            if (lblWriteStatus.InvokeRequired)
            {
                lblWriteStatus.Invoke(new Action(() =>
                {
                    lblWriteStatus.Text = message;
                    lblWriteStatus.ForeColor = color;
                }));
            }
            else
            {
                lblWriteStatus.Text = message;
                lblWriteStatus.ForeColor = color;
            }
            Console.WriteLine($"Status updated: {message}");
        }

        private async void BtnWrite_Click(object sender, EventArgs e)
        {
            UpdateStatusLabel("Writing to card...", UIHelpers.HexToColor("#027f80"));

            try
            {
                using (var reader = new SCardReader(_context))
                {
                    var connection = reader.Connect(_readerName, SCardShareMode.Shared, SCardProtocol.Any);
                    if (connection != SCardError.Success)
                    {
                        UpdateStatusLabel($"Reader error: {SCardHelper.StringifyError(connection)}", Color.Red);
                        return;
                    }

                    string cardType = await DetectCardTypeAsync(reader);
                    Console.WriteLine($"Detected Card Type: {cardType}");

                    string message = txtMessage.Text.Trim();
                    if (string.IsNullOrEmpty(message))
                    {
                        UpdateStatusLabel("Please enter a message to write.", UIHelpers.HexToColor("#027f80"));
                        return;
                    }

                    // Validate based on card type
                    bool isValid = false;
                    switch (cardType)
                    {
                        case "MIFARE_CLASSIC":
                            isValid = ValidateAdminOrUserID(message);
                            break;
                        case "NTAG213":
                            isValid = ValidateAssetID(message);
                            break;
                        default:
                            UpdateStatusLabel("Unsupported or unknown card type.", Color.Red);
                            return;
                    }

                    if (!isValid)
                    {
                        return; // Validation errors are already set in the status label
                    }

                    bool success = false;
                    switch (cardType)
                    {
                        case "MIFARE_CLASSIC":
                            success = await WriteMifareClassicNDEFAsync(reader, message);
                            UpdateStatusLabel(success ? "Message written to MIFARE Classic card!" : "Failed to write to MIFARE card.", success ? Color.Green : Color.Red);
                            break;

                        case "NTAG213":
                            success = await WriteNtag213Async(reader, message);
                            UpdateStatusLabel(success ? "Message written to NTAG213 card!" : "Failed to write to NTAG card.", success ? Color.Green : Color.Red);
                            break;

                        default:
                            UpdateStatusLabel("Unsupported or unknown card type.", Color.Red);
                            break;
                    }

                    reader.Disconnect(SCardReaderDisposition.Reset);
                }
            }
            catch (Exception ex)
            {
                UpdateStatusLabel($"Error: {ex.Message}", Color.Red);
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _monitor?.Cancel();
            _monitor?.Dispose();
            _context?.Dispose();
        }

        public async Task<bool> WriteMifareClassicNDEFAsync(SCardReader reader, string message)
        {
            byte[] keyA = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            // Load Key A into the reader
            byte[] loadKeyCommand = {
                0xFF, 0x82, 0x00, 0x00, 0x06,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
            };
            byte[] loadKeyResp = new byte[2];
            int loadKeyRespLen = loadKeyResp.Length;
            if (reader.Transmit(loadKeyCommand, loadKeyCommand.Length, loadKeyResp, ref loadKeyRespLen) != SCardError.Success || loadKeyResp[0] != 0x90)
            {
                Console.WriteLine("Failed to load key.");
                return false;
            }

            // Construct NDEF message
            byte[] textBytes = Encoding.ASCII.GetBytes(message);
            byte[] languageCode = Encoding.ASCII.GetBytes("en");
            byte statusByte = (byte)languageCode.Length; // 0x02 for "en"
            byte[] payload = new byte[1 + languageCode.Length + textBytes.Length];
            payload[0] = statusByte; // 0x02
            Array.Copy(languageCode, 0, payload, 1, languageCode.Length); // 0x65 0x6E
            Array.Copy(textBytes, 0, payload, 1 + languageCode.Length, textBytes.Length);

            byte[] ndefMessage = new byte[4 + payload.Length];
            ndefMessage[0] = 0xD1; // MB=1, ME=1, SR=1, TNF=0x01
            ndefMessage[1] = 0x01; // Type Length
            ndefMessage[2] = (byte)payload.Length; // Payload Length
            ndefMessage[3] = 0x54; // Type 'T'
            Array.Copy(payload, 0, ndefMessage, 4, payload.Length);

            // Check NDEF message length
            if (ndefMessage.Length > 255)
            {
                Console.WriteLine("NDEF message too long for TLV length field.");
                return false;
            }

            // Construct TLV
            byte[] tlv = new byte[2 + ndefMessage.Length + 1];
            tlv[0] = 0x03; // NDEF Message TLV Tag
            tlv[1] = (byte)ndefMessage.Length; // Length
            Array.Copy(ndefMessage, 0, tlv, 2, ndefMessage.Length);
            tlv[tlv.Length - 1] = 0xFE; // Terminator TLV

            // Check TLV size
            if (tlv.Length > 704)
            {
                Console.WriteLine("Message too large for MIFARE Classic 1K.");
                return false;
            }

            // Pad to 16-byte blocks
            int paddedLen = ((tlv.Length + 15) / 16) * 16;
            Array.Resize(ref tlv, paddedLen);

            int written = 0;

            // Loop over sectors (1 to 15)
            for (int sector = 1; sector < 16; sector++)
            {
                byte sectorStartBlock = (byte)(sector * 4);

                // Loop over blocks in the sector (0 to 2, skip trailer block)
                for (int blockOffset = 0; blockOffset < 3; blockOffset++)
                {
                    byte block = (byte)(sectorStartBlock + blockOffset);

                    // Authenticate block
                    byte[] authCommand = {
                        0xFF, 0x86, 0x00, 0x00, 0x05,
                        0x01, 0x00, block, 0x60, 0x00
                    };
                    byte[] authResp = new byte[2];
                    int authLen = authResp.Length;

                    if (reader.Transmit(authCommand, authCommand.Length, authResp, ref authLen) != SCardError.Success || authResp[0] != 0x90)
                    {
                        Console.WriteLine($"Authentication failed for block {block}");
                        continue;
                    }

                    if (written >= tlv.Length) break;

                    // Write command: 5 bytes header + 16 bytes data
                    byte[] writeCommand = new byte[5 + 16];
                    writeCommand[0] = 0xFF;
                    writeCommand[1] = 0xD6; // Write command
                    writeCommand[2] = 0x00;
                    writeCommand[3] = block;
                    writeCommand[4] = 0x10; // Write 16 bytes

                    // Calculate bytes to copy
                    int bytesToCopy = Math.Min(16, tlv.Length - written);
                    if (bytesToCopy <= 0)
                    {
                        Console.WriteLine($"Invalid bytesToDSCopy ({bytesToCopy}) at block {block}.");
                        return false;
                    }
                    Array.Copy(tlv, written, writeCommand, 5, bytesToCopy);

                    byte[] dataBuffer = new byte[64];
                    int recvLen = dataBuffer.Length;

                    if (reader.Transmit(writeCommand, writeCommand.Length, dataBuffer, ref recvLen) != SCardError.Success || dataBuffer[0] != 0x90)
                    {
                        Console.WriteLine($"Write failed at block {block}");
                        return false;
                    }

                    written += bytesToCopy;
                }

                if (written >= tlv.Length) break;
            }

            return true;
        }

        public async Task<bool> WriteNtag213Async(SCardReader reader, string message)
        {
            try
            {
                // Construct NDEF Text record payload
                byte[] textBytes = Encoding.ASCII.GetBytes(message);
                byte[] languageCode = Encoding.ASCII.GetBytes("en");
                byte statusByte = (byte)languageCode.Length; // 0x02 for "en"
                byte[] payload = new byte[1 + languageCode.Length + textBytes.Length];
                payload[0] = statusByte; // 0x02
                Array.Copy(languageCode, 0, payload, 1, languageCode.Length); // 0x65 0x6E
                Array.Copy(textBytes, 0, payload, 1 + languageCode.Length, textBytes.Length);

                // Construct NDEF message
                byte[] ndefMessage = new byte[4 + payload.Length];
                ndefMessage[0] = 0xD1; // MB=1, ME=1, SR=1, TNF=0x01
                ndefMessage[1] = 0x01; // Type Length
                ndefMessage[2] = (byte)payload.Length; // Payload Length
                ndefMessage[3] = 0x54; // Type 'T'
                Array.Copy(payload, 0, ndefMessage, 4, payload.Length);

                // Check NDEF message length
                if (ndefMessage.Length > 255)
                {
                    Console.WriteLine("NDEF message too long for TLV length field.");
                    return false;
                }

                // Construct TLV
                byte[] tlv = new byte[2 + ndefMessage.Length + 1];
                tlv[0] = 0x03; // NDEF Message TLV Tag
                tlv[1] = (byte)ndefMessage.Length; // Length
                Array.Copy(ndefMessage, 0, tlv, 2, ndefMessage.Length);
                tlv[tlv.Length - 1] = 0xFE; // Terminator TLV

                // Check if TLV fits in NTAG213 user memory (144 bytes, pages 4-39)
                if (tlv.Length > 144)
                {
                    Console.WriteLine("Message too large for NTAG213 memory.");
                    return false;
                }

                // Pad TLV toFarewell to multiple of 4 bytes (page size)
                int paddedLen = ((tlv.Length + 3) / 4) * 4;
                Array.Resize(ref tlv, paddedLen);

                // Write to pages starting at page 4
                byte startPage = 4;
                int pageCount = paddedLen / 4;
                var sendPci = SCardPCI.GetPci(reader.ActiveProtocol);

                for (int i = 0; i < pageCount; i++)
                {
                    byte[] pageData = new byte[4];
                    int offset = i * 4;

                    // Copy up to 4 bytes from TLV
                    for (int j = 0; j < 4; j++)
                    {
                        pageData[j] = offset + j < tlv.Length ? tlv[offset + j] : (byte)0x00;
                    }

                    // Write command: FF D6 00 <page> 04 <data>
                    byte[] command = new byte[5 + 4];
                    command[0] = 0xFF;
                    command[1] = 0xD6; // Write command
                    command[2] = 0x00;
                    command[3] = (byte)(startPage + i); // Page number
                    command[4] = 0x04; // Write 4 bytes
                    Array.Copy(pageData, 0, command, 5, 4);

                    byte[] receiveBuffer = new byte[256];
                    int receiveLength = receiveBuffer.Length;

                    var result = reader.Transmit(
                        sendPci,
                        command,
                        command.Length,
                        null,
                        receiveBuffer,
                        ref receiveLength
                    );

                    if (result != SCardError.Success || receiveBuffer[0] != 0x90 || receiveBuffer[1] != 0x00)
                    {
                        Console.WriteLine($"Failed to write page {startPage + i}: {result}");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to NTAG213: {ex.Message}");
                return false;
            }
        }

        private async Task<string> DetectCardTypeAsync(SCardReader reader)
        {
            byte[] getUIDCommand = new byte[] { 0xFF, 0xCA, 0x00, 0x00, 0x00 }; // APDU command to get UID
            byte[] response = new byte[256];
            int responseLength = response.Length;

            SCardError sendResult = reader.Transmit(getUIDCommand, getUIDCommand.Length, response, ref responseLength);

            if (sendResult == SCardError.Success && responseLength > 2)
            {
                byte[] uidBytes = new byte[responseLength - 2];
                Array.Copy(response, 0, uidBytes, 0, uidBytes.Length);

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

        private bool ValidateAdminOrUserID(string id)
        {
            if (id.Length != 8 || !long.TryParse(id, out long idNumber))
            {
                UpdateStatusLabel("Error: Admin/User ID must be an 8-digit number.", Color.Red);
                Console.WriteLine($"Admin/User ID validation failed: {id} is not an 8-digit number.");
                return false;
            }

            if (idNumber < 10000000 || idNumber > 99999999)
            {
                UpdateStatusLabel("Error: Admin/User ID must be between 10000000 and 99999999.", Color.Red);
                Console.WriteLine($"Admin/User ID validation failed: {id} is out of range (10000000-99999999).");
                return false;
            }

            return true;
        }

        private bool ValidateAssetID(string assetId)
        {
            if (!IsValidUUID(assetId))
            {
                UpdateStatusLabel("Error: Asset ID must be a valid UUID.", Color.Red);
                Console.WriteLine($"Asset ID validation failed: {assetId} is not a valid UUID.");
                return false;
            }

            return true;
        }

        private bool IsValidUUID(string uuid)
        {
            string pattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
            return System.Text.RegularExpressions.Regex.IsMatch(uuid, pattern);
        }
    }
}