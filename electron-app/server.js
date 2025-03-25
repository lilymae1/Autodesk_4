// const express = require("express");
// const fs = require("fs");
// const cors = require("cors");
// const app = express();

// app.use(express.json());
// app.use(cors());

// const CHAT_LOG_FILE = "chat_log.txt";

// // Append message to chat log file
// app.post("/saveMessage", (req, res) => {
//     const { message } = req.body;
    
//     if (!message) {
//         return res.status(400).json({ error: "Message is required" });
//     }

//     const timestamp = new Date().toISOString();
//     const logEntry = `${timestamp}: ${message}\n`;

//     // Append message to the file
//     fs.appendFile(CHAT_LOG_FILE, logEntry, (err) => {
//         if (err) {
//             return res.status(500).json({ error: "Failed to save message" });
//         }
//         res.json({ success: true });
//     });
// });

// // Retrieve chat log
// app.get("/getChatLog", (req, res) => {
//     fs.readFile(CHAT_LOG_FILE, "utf8", (err, data) => {
//         if (err) {
//             return res.status(500).json({ error: "Failed to load chat log" });
//         }
//         res.json({ chatLog: data });
//     });
// });

// const PORT = 3000;
// app.listen(PORT, () => console.log(`Server running on http://localhost:${PORT}`));

const express = require("express");
const fs = require("fs");
const cors = require("cors");
const app = express();

app.use(express.json());
app.use(cors());

const CHAT_LOG_FILE = "chat_log.txt";
const PROJECTS_FILE = "projects.json";

// Append message to chat log file
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

// Create a new project
app.post("/api/chat/create-project", (req, res) => {
    const { name, description } = req.body;

    if (!name) {
        return res.status(400).json({ error: "Project name is required" });
    }

    const newProject = { Name: name, Description: description || "" };

    fs.readFile(PROJECTS_FILE, "utf8", (err, data) => {
        const projects = err ? [] : JSON.parse(data);
        projects.push(newProject);

        fs.writeFile(PROJECTS_FILE, JSON.stringify(projects, null, 2), (err) => {
            if (err) {
                return res.status(500).json({ error: "Failed to save project" });
            }
            res.json({ message: "Project created successfully", project: newProject });
        });
    });
});

// Retrieve all projects
app.get("/api/chat/projects", (req, res) => {
    fs.readFile(PROJECTS_FILE, "utf8", (err, data) => {
        if (err) {
            return res.status(500).json({ error: "Failed to load projects" });
        }
        res.json(JSON.parse(data));
    });
});

const PORT = 5000;
app.listen(PORT, () => console.log(`Server running on http://localhost:${PORT}`));
