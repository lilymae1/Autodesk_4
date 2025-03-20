const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');
const http = require('http'); // Using native HTTP module

console.log('🚀 Electron app starting...');

function createWindow() {
    const win = new BrowserWindow({
        width: 800,
        height: 600,
        webPreferences: {
            nodeIntegration: false,
            contextIsolation: true,
            preload: path.join(__dirname, 'preload.js')
        }
    });

    win.loadFile(path.join(__dirname, 'UI', 'chatEnlarged.html'));
}

app.whenReady().then(createWindow);

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') {
        app.quit();
    }
});

// Handling messages from the UI
ipcMain.on('chat-message', (event, userInput) => {
    console.log('Received user input:', userInput);

    const requestData = JSON.stringify({ message: userInput });

    const options = {
        hostname: 'localhost',
        port: 5000,
        path: '/api/chatbot/getResponse',
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Content-Length': requestData.length
        }
    };

    const req = http.request(options, (res) => {
        let data = '';

        res.on('data', (chunk) => {
            data += chunk;
        });

        res.on('end', () => {
          try {
              const jsonData = JSON.parse(data);
              console.log('Received response from backend:', jsonData.response);
              
              // Sending the response back to renderer process
              event.reply('chat-response', jsonData.response || 'Error: Unexpected response format.');
              console.log('Response sent back to renderer:', jsonData.response);
          } catch (error) {
              console.error('Error parsing JSON:', error);
              event.reply('chat-response', 'Error: Invalid response format.');
          }
      });
    });

    req.on('error', (error) => {
        console.error("Error contacting .NET backend:", error.message);
        event.reply('chat-response', 'Error: Unable to connect to the backend.');
    });

    req.write(requestData);
    req.end();
});
