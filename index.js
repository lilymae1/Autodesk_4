const { app, BrowserWindow, ipcMain } = require('electron');
const { spawn } = require('child_process');
const path = require('path');

function createWindow() {
  const win = new BrowserWindow({
    width: 800,
    height: 600,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false,
      preload: path.join(__dirname, 'renderer.js') // Preload script for UI logic
    }
  });

  win.loadFile('chatEnlarged.html');
}

app.whenReady().then(createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

// Handling messages from the UI
ipcMain.on('chat-message', (event, userInput) => {
  const chatbotProcess = spawn('dotnet', ['Autodesk_4/bin/Release/net7.0/chatbot.dll', userInput]);

  chatbotProcess.stdout.on('data', (data) => {
    event.reply('chat-response', data.toString());
  });

  chatbotProcess.stderr.on('data', (data) => {
    console.error(`Chatbot error: ${data}`);
  });
});
