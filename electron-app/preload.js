const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('chatAPI', {
  sendMessage: async (userMessage) => {
    try {
      const response = await fetch('http://localhost:5000/api/chatbot/getResponse', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: userMessage })
      });

      const data = await response.json();
      ipcRenderer.send('chat-message', data.response); // Send response back to frontend
    } catch (error) {
      console.error('Error communicating with backend:', error);
      ipcRenderer.send('chat-message', 'Error: Unable to connect to the backend.');
    }
  }
});

window.chatAPI = {
    sendMessage: async (message) => {
        try {
            const response = await fetch('http://localhost:5000/api/chatbot/getResponse', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message })
            });
            const data = await response.json();
            return data.response;
        } catch (error) {
            console.error('Error contacting .NET backend:', error);
        }
    }
};

