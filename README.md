1. Attach a RFID/NFC scanner that works at 13.56MHz range (e.g. ACR122U)
2. Run the application.
3. Go to Settings and confirm the server URI.
4. Scan Admin ID.
5. Now the application is authenticated and asset checkin/checkout can be done by scanning User ID and Asset ID.
6. After successful scanning, either User ID can be resetted by pressing "Reset User" button or Admin can be deauthenticated by pressing "Deauthenticate" button.

Notes
- Admin ID and User ID uses 8-digit employee_num of the user (e.g. 10000010).
- Asset ID uses UUID (e.g. abcdefgh-1234-jkjl-5678-mnopqrstuvwx).
- When writing data, user must put the card/tag on the scanner and then write the data.
