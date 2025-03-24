//Current Improvements / Work Done to make AI do more interaction:
const { app, BrowserWindow, ipcMain, screen } = require('electron');
const path = require('path');
const http = require('http'); // Using native HTTP module
const axios = require('axios');  // Import axios

app.whenReady().then(() => {
  console.log('Electron app is ready');

  const { width, height } = screen.getPrimaryDisplay().workAreaSize; // Move this inside `whenReady`

  function createWindow() {
    const win = new BrowserWindow({
      width: width,
      height: height,
      frame: true,    
      resizable: false, 
      webPreferences: {
        nodeIntegration: false,
        contextIsolation: true,
        preload: path.join(__dirname, 'preload.js')
      }
    });

    win.loadFile(path.join(__dirname, 'UI', 'index.html'));

    win.webContents.on('did-finish-load', () => {
      win.webContents.send('apply-dragging');
    });
  }

  createWindow();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
});


// Close the app when all windows are closed
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

// Handling messages from the UI and integrating with either Ollama API or your chatbot backend API
ipcMain.on('chat-message', async (event, userInput, useOllama = false) => {
  console.log('User input:', userInput);

  try {
    let response;
    if (useOllama) {
      // Use Ollama API
      response = await axios.post('http://localhost:11434/api/generate', {
        input: userInput
      }, {
        headers: {
          'Authorization': 'Bearer your_api_key_here', // Replace with your Ollama API key
          'Content-Type': 'application/json'
        }
      });

      const ollamaResponse = response.data.response; // Adjust based on Ollama's API response structure
      console.log('Ollama AI Response:', ollamaResponse);

      // Send the response back to the renderer (chat window)
      event.reply('chat-response', ollamaResponse);
    } else {
      // Use your backend chatbot API
      response = await axios.post('http://localhost:5000/api/chatbot/getResponse', {
        message: userInput
      });

      const botResponse = response.data.response;
      console.log('Chatbot API Response:', botResponse);

      // Send response back to UI
      event.reply('chat-response', botResponse);
    }
  } catch (error) {
    console.error('Error communicating with API:', error);
    event.reply('chat-response', 'Error: Unable to process your request.');
  }
});

// Window control events
ipcMain.on('minimize-chat', () => {
  let win = BrowserWindow.getFocusedWindow();
  if (win) {
    win.setResizable(true);
    win.setSize(400, 600); // Adjust the minimized window size
    win.setResizable(false);
  }
});

ipcMain.on('fullscreen-chat', () => {
  let win = BrowserWindow.getFocusedWindow();
  if (win) {
    const { width, height } = screen.getPrimaryDisplay().workAreaSize;
    win.setResizable(true);
    win.setSize(width, height);
    win.setBounds({ x: 0, y: 0, width: width, height: height });
    win.setResizable(false);
  }
});

ipcMain.on('move-window', (event, dx, dy) => {
  let win = BrowserWindow.getFocusedWindow();
  if (win) {
    const currentBounds = win.getBounds();
    win.setBounds({
      x: currentBounds.x + dx,
      y: currentBounds.y + dy,
      width: currentBounds.width,
      height: currentBounds.height
    });
  }
});