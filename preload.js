const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('chatAPI', {
    sendMessage: (message) => ipcRenderer.send('chat-message', message),
    onResponse: (callback) => ipcRenderer.on('chat-response', (event, message) => callback(message))
});
