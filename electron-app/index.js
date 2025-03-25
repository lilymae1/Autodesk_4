// Current Improvements / Work Done to make AI do more interaction:
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
    // Simulate the response (for testing purposes)
   

    // Check if the response contains the "CreateWall" command
    if (response.data.command && response.data.command === "CreateWall") {
      // If the response contains the "CreateWall" command, forward it for execution
      console.log('Detected Revit Command! Forwarding to Revit API...');
      
      // Construct the Revit command with parameters
      const revitCommand = {
        type: "createWall",
        parameters: response.data.parameters // Pass parameters directly
      };

      // Send a response to the UI to indicate the command is being executed
      event.reply('chat-response', "Executing Revit command...");

      // Forward the command to the Revit API for execution
      ipcMain.emit('execute-revit-command', null, revitCommand);
    } else {
      // If the response does not contain a Revit command, just send the response to the UI
      event.reply('chat-response', response.data.response);
    }

  } catch (error) {
    // Log any errors that occur during the API call
    console.error('Error communicating with API:', error);
    event.reply('chat-response', 'Error: Unable to process your request.');
  }
});


// Handling structured Revit commands and sending them to the Revit API
ipcMain.on('execute-revit-command', async (event, revitCommand) => {
  try {
    const revitResponse = await axios.post("http://localhost:5000/api/revit/execute", revitCommand);
    console.log("Revit API Response:", revitResponse.data);
    
    // Send Revit's response back to the UI
    event.reply("chat-response", revitResponse.data);
  } catch (error) {
    console.error("Error executing Revit command:", error);
    event.reply("chat-response", "Error executing Revit command.");
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
