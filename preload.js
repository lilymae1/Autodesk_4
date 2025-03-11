const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('chatAPI', {
    sendMessage: (message) => ipcRenderer.send('chat-message', message),
    onResponse: (callback) => ipcRenderer.on('chat-response', (event, message) => callback(message))

});

document.getElementById('sendButton').addEventListener('click', () => {
    const userInput = document.getElementById('userInput').value;
    if (userInput.trim()) {
      appendMessage('user', userInput);
      ipcRenderer.send('chat-message', userInput);
      document.getElementById('userInput').value = '';
    }
  });
  
ipcRenderer.on('chat-response', (event, message) => {
    appendMessage('bot', message);
});

function appendMessage(sender, message) {
    const chatlog = document.getElementById('chatlog');
    const msgDiv = document.createElement('div');
    msgDiv.classList.add(sender + '-msg');
    msgDiv.innerText = message;
    chatlog.appendChild(msgDiv);
    chatlog.scrollTop = chatlog.scrollHeight;
}

