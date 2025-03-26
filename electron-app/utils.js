const fs = require('fs');

module.exports.readJsonFile = (filePath) => {
    if (!fs.existsSync(filePath)) {
        return {};
    }
    return JSON.parse(fs.readFileSync(filePath, 'utf-8'));
};