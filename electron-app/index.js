const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');

function createWindow() {
  const win = new BrowserWindow({
    width: 800,
    height: 600,
    webPreferences: {
      nodeIntegration: false, // Ensure nodeIntegration is false for security
      contextIsolation: true, // Ensure contextIsolation is enabled for security
      preload: path.join(__dirname, 'preload.js') // Preload script for UI logic
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
  console.log('User input:', userInput);

  // Send a fixed response for now as a test
  const response = "Hello, I am Archie! How can I assist you today?";
  
  // Send the response back to the renderer (chat window)
  event.reply('chat-response', response);
});
