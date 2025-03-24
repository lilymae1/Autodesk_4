//Current Update to making AI do more than simple interaction
const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('chatAPI', {
  sendMessage: (userMessage) => {
    ipcRenderer.send('chat-message', userMessage);
  },

  // For receiving the chatbot's response
  onResponse: (callback) => {
    ipcRenderer.on('chat-response', (_, response) => {
      callback(response);
    });
  },

  // Window control functions
  minimizeChat: () => ipcRenderer.send('minimize-chat'),
  fullscreenChat: () => ipcRenderer.send('fullscreen-chat'),
  moveWindow: (dx, dy) => ipcRenderer.send('move-window', dx, dy)
});