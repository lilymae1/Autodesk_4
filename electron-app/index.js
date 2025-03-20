const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');
const axios = require('axios');  // Import axios

// Create the Electron window
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

// Close the app when all windows are closed
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

// Handling messages from the UI and integrating with Ollama API
ipcMain.on('chat-message', async (event, userInput) => {
  console.log('User input:', userInput);

  try {
    // Send the user input to Ollama API
    const response = await axios.post('http://localhost:11434/api/generate', {
      input: userInput  // Modify according to Ollama's API documentation
    }, {
      headers: {
        'Authorization': 'Bearer your_api_key_here', // Replace with your Ollama API key
        'Content-Type': 'application/json'
      }
    });

    // Extract the relevant response from Ollama's API (modify based on actual response format)
    const ollamaResponse = response.data.response; // Adjust based on Ollama's API response structure
    console.log('Ollama AI Response:', ollamaResponse);

    // Send the response back to the renderer (chat window)
    event.reply('chat-response', ollamaResponse);

  } catch (error) {
    console.error('Error communicating with Ollama:', error);
    event.reply('chat-response', 'Sorry, something went wrong. Please try again.');
  }
});
