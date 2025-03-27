const fs = require('fs');
const path = require('path');
const { getProjectPath } = require('./utils');
const { exec } = require('child_process');

// Create a new project
exports.createProject = (req, res) => {
    const { name, description } = req.body;
    const projectPath = getProjectPath(name);

    if (!fs.existsSync(projectPath)) {
        fs.mkdirSync(projectPath, { recursive: true });
        fs.writeFileSync(path.join(projectPath, 'project.json'), JSON.stringify({ name, description }));
        res.json({ message: 'Project created successfully' });
    } else {
        res.status(400).json({ error: 'Project already exists' });
    }
};

// Retrieve all projects
exports.getProjects = (req, res) => {
    const basePath = getProjectPath('');
    const projects = fs.readdirSync(basePath).filter(dir => fs.lstatSync(path.join(basePath, dir)).isDirectory());
    const projectData = projects.map(project => {
        const projectFile = path.join(basePath, project, 'project.json');
        return fs.existsSync(projectFile) ? JSON.parse(fs.readFileSync(projectFile)) : { name: project, description: '' };
    });
    res.json(projectData);
};

// Update an existing project
exports.updateProject = (req, res) => {
    const { name, description } = req.body;
    const projectPath = getProjectPath(name);
    if (fs.existsSync(projectPath)) {
        fs.writeFileSync(path.join(projectPath, 'project.json'), JSON.stringify({ name, description }));
        res.json({ message: 'Project updated successfully' });
    } else {
        res.status(404).json({ error: 'Project not found' });
    }
};

// Open an existing project in Revit
exports.openProject = (req, res) => {
    const { name } = req.body;
    const projectPath = getProjectPath(name);
    if (fs.existsSync(projectPath)) {
        exec(`start "" "C:\\Program Files\\Autodesk\\Revit 2024\\Revit.exe" "${projectPath}\\${name}.rvt"`, (error) => {
            if (error) {
                res.status(500).json({ error: 'Failed to open project in Revit' });
            } else {
                res.json({ message: 'Opening project in Revit' });
            }
        });
    } else {
        res.status(404).json({ error: 'Project not found' });
    }
};