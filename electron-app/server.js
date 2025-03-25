const express = require("express");
const fs = require("fs");
const path = require("path");
const cors = require("cors");
const os = require("os");

const app = express();
app.use(express.json());
app.use(cors());

// Define paths
const ELECTRON_APP_PATH = path.join(__dirname); // Electron app root
const REVIT_APPDATA_PATH = path.join(os.homedir(), "AppData", "Roaming", "Autodesk", "Revit", "Addins", "ChatProjects");
const PROJECTS_FILE = path.join(REVIT_APPDATA_PATH, "projects.json");
const CHAT_LOG_FILE = path.join(ELECTRON_APP_PATH, "chat_log.txt");

// Ensure the Revit AppData directory exists
if (!fs.existsSync(REVIT_APPDATA_PATH)) {
    fs.mkdirSync(REVIT_APPDATA_PATH, { recursive: true });
}

// Ensure projects.json exists
if (!fs.existsSync(PROJECTS_FILE)) {
    fs.writeFileSync(PROJECTS_FILE, "[]", "utf8");
}

// Append message to chat log
app.post("/saveMessage", (req, res) => {
    const { message } = req.body;

    if (!message) {
        return res.status(400).json({ error: "Message is required" });
    }

    const timestamp = new Date().toISOString();
    const logEntry = `${timestamp}: ${message}\n`;

    fs.appendFile(CHAT_LOG_FILE, logEntry, (err) => {
        if (err) {
            return res.status(500).json({ error: "Failed to save message" });
        }
        res.json({ success: true });
    });
});

// Retrieve chat log
app.get("/getChatLog", (req, res) => {
    fs.readFile(CHAT_LOG_FILE, "utf8", (err, data) => {
        if (err) {
            return res.status(500).json({ error: "Failed to load chat log" });
        }
        res.json({ chatLog: data });
    });
});

// Create a new project and save it in Revit's AppData
app.post("/api/chat/create-project", (req, res) => {
    const { name, description } = req.body;

    if (!name) {
        return res.status(400).json({ error: "Project name is required" });
    }

    const projectFolder = path.join(REVIT_APPDATA_PATH, name);

    // Create a folder for the project
    if (!fs.existsSync(projectFolder)) {
        fs.mkdirSync(projectFolder);
    }

    // Save project details in projects.json and a separate JSON file
    const projectFile = path.join(projectFolder, "project.json");
    const newProject = { Name: name, Description: description || "" };

    fs.readFile(PROJECTS_FILE, "utf8", (err, data) => {
        let projects = err ? [] : JSON.parse(data);
        projects.push(newProject);

        // Save to projects.json
        fs.writeFile(PROJECTS_FILE, JSON.stringify(projects, null, 2), (err) => {
            if (err) {
                return res.status(500).json({ error: "Failed to save project" });
            }

            // Also save in the project's folder
            fs.writeFile(projectFile, JSON.stringify(newProject, null, 2), (err) => {
                if (err) {
                    return res.status(500).json({ error: "Failed to save project file" });
                }
                res.json({ message: "Project created successfully", project: newProject });
            });
        });
    });
});

// Retrieve all projects from Revit AppData
app.get("/api/chat/projects", (req, res) => {
    fs.readFile(PROJECTS_FILE, "utf8", (err, data) => {
        if (err) {
            return res.status(500).json({ error: "Failed to load projects" });
        }
        res.json(JSON.parse(data));
    });
});

// Start server
const PORT = 5000;
app.listen(PORT, () => console.log(`Server running on http://localhost:${PORT}`));
