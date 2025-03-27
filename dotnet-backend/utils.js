const path = require('path');

const baseProjectPath = "C:\\Users\\joann\\AppData\\Roaming\\RevitChatProjects";

exports.getProjectPath = (projectName) => {
    return path.join(baseProjectPath, projectName);
};