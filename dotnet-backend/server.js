// server.js
const express = require('express');
const bodyParser = require('body-parser');
const cors = require('cors');
const projectController = require('./ProjectController');
const chatController = require('./chatController');

const app = express();
const PORT = 3000;

app.use(cors());
app.use(bodyParser.json());

// Project API Routes
app.post('/createProject', projectController.createProject);
app.get('/getProjects', projectController.getProjects);
app.post('/updateProject', projectController.updateProject);
app.post('/openProject', projectController.openProject);

// Chat API Routes
app.post('/saveChat', chatController.saveChat);
app.get('/getChat/:projectName', chatController.getChat);

app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});
// const express = require("express");
// const fs = require("fs");
// const path = require("path");
// const cors = require("cors");
// const os = require("os");

// const app = express();
// app.use(express.json());
// app.use(cors());

// // Define paths
// const REVIT_APPDATA_PATH = path.join(os.homedir(), "AppData", "Roaming", "Autodesk", "Revit", "Addins", "ChatProjects");
// const PROJECTS_FILE = path.join(REVIT_APPDATA_PATH, "projects.json");

// // Ensure necessary directories exist
// try {
//     if (!fs.existsSync(REVIT_APPDATA_PATH)) {
//         fs.mkdirSync(REVIT_APPDATA_PATH, { recursive: true });
//     }

//     if (!fs.existsSync(PROJECTS_FILE)) {
//         fs.writeFileSync(PROJECTS_FILE, "[]", { encoding: "utf8", flag: "wx" }); // 'wx' prevents overwriting if it exists
//     }
// } catch (error) {
//     console.error("Error initializing project directory or file:", error);
// }

// // Utility function to load projects from file
// const loadProjects = () => {
//     try {
//         const data = fs.readFileSync(PROJECTS_FILE, "utf8");
//         const projects = JSON.parse(data) || [];
//         return projects;
//     } catch (error) {
//         console.error("Error loading projects.json:", error);
//         return [];
//     }
// };

// // Utility function to save projects to file
// const saveProjects = (projects) => {
//     try {
//         fs.writeFileSync(PROJECTS_FILE, JSON.stringify(projects, null, 2), "utf8");
//         return true;
//     } catch (error) {
//         console.error("Error saving projects.json:", error);
//         return false;
//     }
// };

// // Create a new project
// app.post("/api/chat/create-project", (req, res) => {
//     const { name, description } = req.body;

//     if (!name) {
//         return res.status(400).json({ error: "Project name is required" });
//     }

//     const projectFolder = path.join(REVIT_APPDATA_PATH, name);
//     let projects = loadProjects();

//     if (projects.some(p => p.Name === name)) {
//         return res.status(400).json({ error: "Project with this name already exists" });
//     }

//     try {
//         if (!fs.existsSync(projectFolder)) {
//             fs.mkdirSync(projectFolder, { recursive: true });
//         }

//         const newProject = { Name: name, Description: description || "No description available" };
//         projects.push(newProject);

//         if (!saveProjects(projects)) {
//             return res.status(500).json({ error: "Failed to save project metadata" });
//         }

//         fs.writeFileSync(path.join(projectFolder, "project.json"), JSON.stringify(newProject, null, 2), "utf8");

//         res.json({ message: "Project created successfully", project: newProject });
//     } catch (error) {
//         console.error("Error creating project:", error);
//         res.status(500).json({ error: "Failed to create project" });
//     }
// });

// // Retrieve all projects with correct structure
// app.get("/api/chat/projects", (req, res) => {
//     try {
//         let projects = loadProjects();
//         projects = projects.map(project => ({
//             Name: project.Name || "Unknown",  
//             Description: project.Description || "No description available"  
//         }));

//         res.json(projects);
//     } catch (error) {
//         console.error("Error retrieving projects:", error);
//         res.status(500).json({ error: "Failed to load projects" });
//     }
// });

// // Update project details (name and description)
// app.post("/api/chat/update-project", (req, res) => {
//     const { name, description } = req.body;

//     if (!name) {
//         return res.status(400).json({ error: "Project name is required" });
//     }

//     let projects = loadProjects();

//     const projectIndex = projects.findIndex(project => project.Name === name);
//     if (projectIndex === -1) {
//         return res.status(404).json({ error: "Project not found" });
//     }

//     projects[projectIndex].Description = description;

//     if (!saveProjects(projects)) {
//         return res.status(500).json({ error: "Failed to save project metadata" });
//     }

//     res.json({ message: "Project updated successfully", project: projects[projectIndex] });
// });

// // Start the server
// const PORT = 5000;
// app.listen(PORT, () => console.log(`Server running on http://localhost:${PORT}`));