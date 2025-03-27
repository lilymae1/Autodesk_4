const { app, BrowserWindow, ipcMain, screen } = require('electron');
const path = require('path');
const axios = require('axios');
const express = require('express');
const fs = require('fs');
const cors = require('cors');

// Initialize Express server
const expressApp = express();
const PORT = 3000;
expressApp.use(cors());

const appDataPath = "C:\\Users\\Aaron Wass\\AppData\\Roaming\\RevitChatProjects";
expressApp.use(express.static(path.join(__dirname, 'UI')));

// Route to list files and folders
expressApp.get("/files", (req, res) => {
    fs.readdir(appDataPath, { withFileTypes: true }, (err, items) => {
        if (err) {
            console.error("Error reading directory:", err);
            return res.status(500).json({ error: "Unable to read directory" });
        }
        const filesAndFolders = items.map(item => ({
            name: item.name,
            type: item.isDirectory() ? "folder" : "file"
        }));
        res.json(filesAndFolders);
    });
});

// Start the server
expressApp.listen(PORT, () => {
    console.log(`Server running at http://localhost:${PORT}`);
});

// Initialize Electron app
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
        win.loadURL('http://localhost:3000');
        win.webContents.on('did-finish-load', () => {
            win.webContents.send('apply-dragging');
        });
    }

    createWindow();
    app.on('activate', () => {
        if (BrowserWindow.getAllWindows().length === 0) createWindow();
    });
});

// Close app when all windows are closed
app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') {
        app.quit();
    }
});

// Handle chat messages
ipcMain.on('chat-message', async (event, userInput) => {
    console.log('User input:', userInput);
    try {
        if (userInput.toLowerCase().includes("create wall")) {
            console.log("Detected 'Create Wall' command.");
            const revitCommand = { command: "Create Wall" };
            event.reply('chat-response', "Executing Revit command...");
            ipcMain.emit('execute-revit-command', null, revitCommand);
        } else {
            let response = await axios.post('http://localhost:5000/api/chatbot/getResponse', { message: userInput });
            event.reply('chat-response', response.data.response || "No response from AI.");
        }
    } catch (error) {
        console.error('Error communicating with API:', error);
        event.reply('chat-response', 'Error: Unable to process your request.');
    }
});

// Handle Revit commands
ipcMain.on('execute-revit-command', async (event, revitCommand) => {
    try {
        const revitResponse = await axios.post("http://localhost:5000/api/revit/execute", revitCommand);
        event.reply("chat-response", revitResponse.data);
    } catch (error) {
        console.error("Error executing Revit command:", error);
        event.reply("chat-response", "Error executing Revit command.");
    }
});

// Window controls
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
