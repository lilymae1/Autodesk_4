const { contextBridge, ipcRenderer } = require('electron');

// Expose the chatAPI to the renderer process
contextBridge.exposeInMainWorld('chatAPI', {
  sendMessage: (userMessage) => {
    ipcRenderer.send('chat-message', userMessage); // Send the user message to the main process
  },
  onResponse: (callback) => {
    ipcRenderer.on('chat-response', (event, responseMessage) => {
      callback(responseMessage); // Trigger the callback when the response is received
    });
  }
});

