function formatTimestamp(date) {
    return new Date(date).toLocaleString('en-GB', {
        hour: '2-digit',
        minute: '2-digit',
        day: '2-digit',
        month: 'short',
    }).replace(',', ''); // Remove comma for cleaner format
}

// Updated appendMessage to handle "Thinking..." correctly
function appendMessage(sender, message, timestamp = new Date().toLocaleString(), isThinking = false) {
    const chatlog = document.getElementById('chat');

    const msgDiv = document.createElement('div');
    msgDiv.classList.add(sender === 'user' ? 'outgoing-msg' : 'received-msg');

    const chatsDiv = document.createElement('div');
    chatsDiv.classList.add(sender === 'user' ? 'outgoing-chats' : 'received-chats');

    const msgBox = document.createElement('div');
    msgBox.classList.add(sender === 'user' ? 'outgoing-chats-msg' : 'received-msg-inbox');

    const msgText = document.createElement('p');
    msgText.textContent = message;

    if (isThinking) {
        msgText.classList.add('bot-msg');  // Add class for easier removal
    }

    // Display timestamp if provided
    if (!timestamp) {
        timestamp = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }
    const timeSpan = document.createElement('span');
    timeSpan.classList.add(sender === 'user' ? 'outgoing-time' : 'received-time');
    timeSpan.textContent = timestamp;

    const imgDiv = document.createElement('div');
    imgDiv.className = sender === 'user' ? 'outgoing-chats-img' : 'received-chats-img';
    imgDiv.innerHTML = `<img src="${sender === 'user' ? 'Assets/ProfileIcon.png' : 'Assets/archie.png'}" width="42" height="42" style="border-radius: 21px;">`;

    msgBox.appendChild(msgText);
    msgBox.appendChild(timeSpan);
    chatsDiv.appendChild(msgBox);
    chatsDiv.appendChild(imgDiv);
    msgDiv.appendChild(chatsDiv);

    chatlog.appendChild(msgDiv);
    chatlog.scrollTop = chatlog.scrollHeight;
}

// Send message handler
function send_message(event) {
    event.preventDefault();
    let messageInput = document.getElementById("input");

    if (messageInput.value.trim() === "") return; // Prevent sending empty messages

    let userMessage = messageInput.value;

    // Save and display user message
    appendMessage('user', userMessage);
    // Send "Thinking..." message
    appendMessage('bot', 'Thinking...', true);

    // Send message using Electron's IPC
    if (window.chatAPI) {
        window.chatAPI.sendMessage(userMessage); 
    } else {
        console.error('chatAPI is undefined');
        appendMessage('bot', 'Error: Unable to connect to the backend.');
    }

    messageInput.value = ""; // Clear the input field after sending
}

// Handle bot response from Electron
function archie_message() {
    if (window.chatAPI) {
        window.chatAPI.onResponse((response) => {
            console.log('Received response from backend:', response);

            // Remove "Thinking..." once response is received
            const thinkingMsgDiv = document.querySelector('.bot-msg');
            if (thinkingMsgDiv) {
                thinkingMsgDiv.closest('.received-msg').remove(); 
            }

            // Display Archie's Response
            appendMessage('bot', response);
        });
    } else {
        console.error("chatAPI is undefined inside archie_message");
    }
}

// Welcome message based on time
function sendWelcomeMessage() {
    const currentHour = new Date().getHours();
    let welcomeMessage = "";

    if (currentHour >= 5 && currentHour < 12) {
        welcomeMessage = "Good morning! How can I assist you today?";
    } else if (currentHour >= 12 && currentHour < 18) {
        welcomeMessage = "Good afternoon! How can I help you today?";
    } else {
        welcomeMessage = "Good evening! How can I assist you this evening?";
    }

    appendMessage('bot', welcomeMessage, new Date().toLocaleString());  // Pass timestamp explicitly
}



// Initialize chat when the page loads
document.addEventListener("DOMContentLoaded", () => {
    if (window.chatAPI) {
        console.log("chatAPI is available");
        sendWelcomeMessage();
        archie_message();

    } else {
        console.error("chatAPI is undefined");
    }
});
