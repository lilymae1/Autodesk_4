const { app, BrowserWindow, ipcMain, screen } = require('electron');
const path = require('path');
const fs = require('fs');
const express = require('express');
const axios = require('axios');
const cors = require('cors');

// Initialize Express server
const expressApp = express();
const PORT = 3000;
expressApp.use(cors());

const appDataPath = app.getPath("appData") + "\\RevitChatProjects";
expressApp.use(express.static(path.join(__dirname, 'UI')));

// Route to list of folders and get images
expressApp.get("/files", (req, res) => {
    fs.readdir(appDataPath, { withFileTypes: true }, (err, items) => {
        if (err) {
            console.error("Error reading directory:", err);
            return res.status(500).json({ error: "Unable to read directory" });
        }

        const filesAndFolders = items.map(item => {
            const imagePath = path.join(__dirname, 'UI', 'Assets', `${item.name}.png`);
            const imageExists = fs.existsSync(imagePath);

            return {
                name: item.name,
                type: item.isDirectory() ? "folder" : "file",
                image: imageExists ? `Assets/${item.name}.png` : `Assets/example_image.png`
            };
        });

        res.json(filesAndFolders);
    });
});

// Route to revit projects (Samples folder)
const revitSamplesPath = "C:\\Program Files\\Autodesk\\Revit 2025\\Samples";
expressApp.get('/revit-projects', (req, res) => {
    fs.readdir(revitSamplesPath, (err, files) => {
        if (err) {
            console.error("Error reading directory:", err);
            return res.status(500).json({ error: "Unable to read directory" });
        }

        res.json(files.map(file => ({ name: file })));
    });
});

// Route to create a new project
expressApp.post("/api/chat/create-project", express.json(), (req, res) => {
    const { name, description } = req.body;
    if (!name) {
        return res.status(400).json({ error: "Project name is required" });
    }

    const projectPath = path.join(appDataPath, name);
    const infoPath = path.join(projectPath, "info.json");

    if (!fs.existsSync(projectPath)) {
        fs.mkdirSync(projectPath, { recursive: true });
    }

    const projectData = { name, description };
    fs.writeFileSync(infoPath, JSON.stringify(projectData, null, 2));

    // Create a new chat log file when a new project is created
    const chatLogPath = path.join(projectPath, "chatlog.txt");
    fs.writeFileSync(chatLogPath, "Chat started...\n");

    res.json({ message: `Project '${name}' created and chat log initialized` });
});

// Route to create a new chat log
expressApp.post("/api/chat/create-chatlog", express.json(), (req, res) => {
    const { projectName } = req.body;
    if (!projectName) {
        return res.status(400).json({ error: "Project name is required" });
    }

    const projectPath = path.join(appDataPath, projectName);

    if (!fs.existsSync(projectPath)) {
        return res.status(404).json({ error: `Project '${projectName}' not found` });
    }

    const chatLogPath = path.join(projectPath, "chatlog.txt");

    // Create an empty chat log file
    fs.writeFileSync(chatLogPath, "Chat started...\n");

    res.json({ message: `Chat log for '${projectName}' created successfully` });
});

// Route to save a message to the chatlog.txt of the project
expressApp.post("/api/chat/save-message", express.json(), (req, res) => {
    const { name, sender, message, timestamp } = req.body;

    if (!name || !sender || !message || !timestamp) {
        return res.status(400).json({ error: "Missing required fields" });
    }

    const chatLogPath = path.join(appDataPath, name, "chatlog.txt");

    // Append message to the chatlog.txt
    const logMessage = `[${timestamp}] ${sender}: ${message}\n`;
    fs.appendFileSync(chatLogPath, logMessage);

    res.json({ message: "Message saved to chatlog" });
});

// Handle chat messages (from the frontend interface)
ipcMain.on('chat-message', async (event, userInput) => {
    console.log('User input:', userInput);
    try {
        let response = await axios.post('http://localhost:5000/api/chatbot/getResponse', { message: userInput });
        event.reply('chat-response', response.data.response || "No response from AI.");
    } catch (error) {
        console.error('Error communicating with API:', error);
        event.reply('chat-response', 'Error: Unable to process your request.');
    }
});

// Handle Revit commands (via Electron interaction)
ipcMain.on('execute-revit-command', async (event, revitCommand) => {
    try {
        const revitResponse = await axios.post("http://localhost:5000/api/revit/execute", revitCommand);
        event.reply("chat-response", revitResponse.data);
    } catch (error) {
        console.error("Error executing Revit command:", error);
        event.reply("chat-response", "Error executing Revit command.");
    }
});

// Window controls for Electron (minimize, fullscreen, etc.)
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

// Start the Express server
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