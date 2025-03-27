const fs = require('fs');
const path = require('path');
const utils = require('./utils');

module.exports.createProject = (req, res) => {
    const { projectName, description } = req.body;
    const projectId = Date.now(); // Unique ID for each project

    const projectFolder = path.join(__dirname, 'RevitChatProjects', projectId.toString());

    if (fs.existsSync(projectFolder)) {
        return res.status(400).json({ message: 'Project already exists' });
    }

    fs.mkdirSync(projectFolder, { recursive: true });

    const projectData = {
        projectId,
        projectName,
        description,
    };

    const chatHistory = { messages: [] };

    fs.writeFileSync(path.join(projectFolder, 'project.json'), JSON.stringify(projectData, null, 2));
    fs.writeFileSync(path.join(projectFolder, 'chatHistory.json'), JSON.stringify(chatHistory, null, 2));

    return res.status(201).json({ projectId, message: 'Project created successfully' });
};

module.exports.getAllProjects = (req, res) => {
    const projectsDir = path.join(__dirname, 'RevitChatProjects');
    const projects = fs.readdirSync(projectsDir).map((projectId) => {
        const projectFile = path.join(projectsDir, projectId, 'project.json');
        return utils.readJsonFile(projectFile);
    });

    return res.status(200).json(projects);
};

module.exports.getProjectById = (req, res) => {
    const projectId = req.params.id;
    const projectFolder = path.join(__dirname, 'RevitChatProjects', projectId.toString());

    if (!fs.existsSync(projectFolder)) {
        return res.status(404).json({ message: 'Project not found' });
    }

    const projectFile = path.join(projectFolder, 'project.json');
    const projectData = utils.readJsonFile(projectFile);

    return res.status(200).json(projectData);
};