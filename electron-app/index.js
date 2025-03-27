const { app, BrowserWindow, ipcMain, screen } = require('electron');
const path = require('path');
const axios = require('axios');  // Import axios

app.whenReady().then(() => {
  console.log('Electron app is ready');

  const { width, height } = screen.getPrimaryDisplay().workAreaSize;

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
ipcMain.on('chat-message', async (event, userInput) => {
  console.log('User input:', userInput);

  try {
    // Check if the input contains a known command, like "Create Wall"
    if (userInput.toLowerCase().includes("create wall")) {
      console.log("Detected 'Create Wall' command.");

      // Construct the Revit Command based on the detected input
      

      // Send the structured Revit command
      event.reply('chat-response', "Executing Revit command...");

      // Forward command to Revit for execution
      ipcMain.emit('execute-revit-command', null, revitCommand);
    } else {
      // For non-command queries, send a natural language response
      let response = await axios.post('http://localhost:5000/api/chatbot/getResponse', {
        message: userInput
      });

      console.log('Chatbot API Response:', response.data);

      // Handle the regular natural response
      event.reply('chat-response', response.data.response || "No response from AI.");
    }
  } catch (error) {
    console.error('Error communicating with API:', error);
    event.reply('chat-response', 'Error: Unable to process your request.');
  }
});


ipcMain.on('execute-revit-command', async (event, revitCommand) => {
  try {
    const revitResponse = await axios.post("http://localhost:5000/api/revit/execute", revitCommand);
    
    // Ensure Revit API response is valid
    if (!revitResponse.data || Object.keys(revitResponse.data).length === 0) {
      console.warn("Revit API returned an empty response.");
      event.reply("chat-response", "Revit API executed the command but returned no response.");
      return;
    }

    console.log("Revit API Response:", revitResponse.data);
    
    // Send Revit's response back to the UI
    event.reply("chat-response", revitResponse.data);
  } catch (error) {
    console.error("Error executing Revit command:", error.message);
    
    let errorMessage = "Error executing Revit command.";
    if (error.response) {
      errorMessage += ` HTTP ${error.response.status}: ${error.response.statusText}`;
    }

    event.reply("chat-response", errorMessage);
  }
});

// Window control events
ipcMain.on('minimize-chat', () => {
  let win = BrowserWindow.getFocusedWindow();
  if (win) {
    win.setResizable(true);
    win.setSize(400, 600);
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

