const fs = require('fs');
const path = require('path');
const utils = require('./utils');

module.exports.createChat = (req, res) => {
    const { projectId, message } = req.body;
    const projectFolder = path.join(__dirname, 'RevitChatProjects', projectId.toString());

    if (!fs.existsSync(projectFolder)) {
        return res.status(404).json({ message: 'Project not found' });
    }

    const chatFile = path.join(projectFolder, 'chatHistory.json');
    const chatData = utils.readJsonFile(chatFile);
    
    chatData.messages.push({ message, timestamp: new Date() });
    fs.writeFileSync(chatFile, JSON.stringify(chatData, null, 2));

    return res.status(200).json({ message: 'Chat saved successfully' });
};

module.exports.getChat = (req, res) => {
    const projectId = req.params.id;
    const projectFolder = path.join(__dirname, 'RevitChatProjects', projectId.toString());

    if (!fs.existsSync(projectFolder)) {
        return res.status(404).json({ message: 'Project not found' });
    }

    const chatFile = path.join(projectFolder, 'chatHistory.json');
    const chatData = utils.readJsonFile(chatFile);

    return res.status(200).json(chatData);
};