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

const appDataPath = app.getPath("appData") + "\\RevitChatProjects";
expressApp.use(express.static(path.join(__dirname, 'UI')));

// Route to list of folders and get image
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


expressApp.delete('/delete-folder', express.json(), (req, res) => {
    const { folderName } = req.body;

    if (!folderName) {
        return res.status(400).json({ error: 'Folder name is required' });
    }

    const dirPath = path.join(appDataPath, folderName);
    
    console.log(`Attempting to delete: ${dirPath}`);

    fs.rm(dirPath, { recursive: true, force: true }, (err) => {
        if (err) {
            console.error("Error deleting folder:", err);
            return res.status(500).json({ error: "Failed to delete folder." });
        }
        console.log("Folder deleted successfully.");
        res.json({ message: "Folder deleted successfully." });
    });
});

// Route to list projects (folders) for importing
expressApp.get("/files", (req, res) => {
    fs.readdir(appDataPath, { withFileTypes: true }, (err, items) => {
        if (err) {
            console.error("Error reading directory:", err);
            return res.status(500).json({ error: "Unable to read directory" });
        }

        const projects = items
            .filter(item => item.isDirectory())
            .map(item => {
                const projectPath = path.join(appDataPath, item.name, "info.json");
                let projectInfo = { name: item.name, description: "No description" };

                if (fs.existsSync(projectPath)) {
                    try {
                        projectInfo = JSON.parse(fs.readFileSync(projectPath, "utf8"));
                    } catch (error) {
                        console.error(`Error reading ${item.name}/info.json:`, error);
                    }
                }

                return {
                    name: projectInfo.name,
                    description: projectInfo.description,
                    path: item.name
                };
            });

        res.json(projects);
    });
});

// Route to create a new project
expressApp.post("/api/chat/create-project", express.json(), (req, res) => {
    //const { name, description ,image} = req.body;
    const { name, description} = req.body;
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

    //var pathz = path.join(projectPath,name+".png") this haunts me like deaths haunts the elderly
    //console.log(pathz)
    //fs.writeFile(pathz,image)

    res.json({ message: `Project '${name}' created successfully` });
});

// Route to update an existing project's name and description
expressApp.post("/api/chat/update-project", express.json(), (req, res) => {
    const { oldName, newName, newDescription } = req.body;
    if (!oldName || !newName) {
        return res.status(400).json({ error: "Both old and new project names are required" });
    }

    const oldProjectPath = path.join(appDataPath, oldName);
    const newProjectPath = path.join(appDataPath, newName);
    const infoPath = path.join(newProjectPath, "info.json");

    if (!fs.existsSync(oldProjectPath)) {
        return res.status(404).json({ error: "Original project not found" });
    }

    if (oldName !== newName && fs.existsSync(newProjectPath)) {
        return res.status(400).json({ error: "A project with the new name already exists" });
    }

    if (oldName !== newName) {
        fs.renameSync(oldProjectPath, newProjectPath);
    }

    const projectData = { name: newName, description: newDescription };
    fs.writeFileSync(infoPath, JSON.stringify(projectData, null, 2));

    res.json({ message: `Project '${oldName}' updated successfully` });
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
    try 
    { 
        let response = await axios.post('http://localhost:5000/api/chatbot/getResponse', { message: userInput });
        event.reply('chat-response', response.data.response || "No response from AI.");
        
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
