// const { contextBridge, ipcRenderer } = require('electron');

// contextBridge.exposeInMainWorld('chatAPI', {
//     sendMessage: async (userMessage) => {
//       try {
//           const response = await fetch('http://localhost:5000/api/chatbot/getResponse', {
//               method: 'POST',
//               headers: { 'Content-Type': 'application/json' },
//               body: JSON.stringify({ message: userMessage }) // Ensure correct JSON structure
//           });
//           const data = await response.json();
//           console.log('Received response from backend:', data);

//           // Ensure response format matches what your frontend expects
//           const botResponse = data.response || 'Error: Unexpected response format.';
//           console.log(botResponse);
//           ipcRenderer.send('chat-message', botResponse); // Send response back to frontend

//           return botResponse;
//       } catch (error) {
//           console.error('Error contacting .NET backend:', error);
//           ipcRenderer.send('chat-message', 'Error: Unable to connect to the backend.');
//           return 'Error: Unable to connect to backend.';
//       }
//     },

//     // For real-time updates from backend to frontend
//     onResponse: (callback) => {
//         ipcRenderer.on('chat-response', (_, response) => {
//             console.log('Response received in frontend:', response);
//             callback(response);
//         });
//     },
//     minimizeChat: () => ipcRenderer.send('minimize-chat'), 
//     fullscreenChat: () => ipcRenderer.send('fullscreen-chat')
// });

//Current Update to making AI do more than simple interaction
const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('chatAPI', {
  sendMessage: async (userMessage, useOllama = false) => {
    try {
      let response, data;
      
      if (useOllama) {
        // Use Ollama API
        response = await fetch('http://localhost:5000/api/generate', {
          method: 'POST',
          headers: { 
            'Content-Type': 'application/json',
            'Authorization': 'Bearer your_api_key_here' // Replace with your actual API key
          },
          body: JSON.stringify({ input: userMessage })
        });
      } else {
        // Use your chatbot backend API
        response = await fetch('http://localhost:5000/api/chatbot/getResponse', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ message: userMessage })
        });
      }

      data = await response.json();
      console.log('Received response:', data);

      // Extract response message
      const botResponse = data.response || 'Error: Unexpected response format.';
      console.log(botResponse);

      ipcRenderer.send('chat-message', botResponse); // Send response back to frontend

      return botResponse;
    } catch (error) {
      console.error('Error contacting API:', error);
      ipcRenderer.send('chat-message', 'Error: Unable to connect to API.');
      return 'Error: Unable to connect to API.';
    }
  },

  // For real-time updates from backend to frontend
  onResponse: (callback) => {
    ipcRenderer.on('chat-response', (_, response) => {
      console.log('Response received in frontend:', response);
      callback(response);
    });
  },

  // Window control functions
  minimizeChat: () => ipcRenderer.send('minimize-chat'),
  fullscreenChat: () => ipcRenderer.send('fullscreen-chat'),
  moveWindow: (dx, dy) => ipcRenderer.send('move-window', dx, dy)
});