// Save messages to localStorage and also to server-side chatlog.txt
// function saveMessageToFile(sender, message) {
//     let messages = JSON.parse(localStorage.getItem("chatMessages")) || [];
    
//     let newMessage = {
//         sender: sender,
//         text: message,
//         timestamp: new Date().toLocaleString()  // Save time message was sent
//     };

//     messages.push(newMessage);
//     localStorage.setItem("chatMessages", JSON.stringify(messages));

//     // Send the message to the server to save in the chatlog.txt
//     const projectName = getCurrentProjectName();
//     if (projectName) {
//         fetch(`/api/chat/save-message`, {
//             method: 'POST',
//             headers: { 'Content-Type': 'application/json' },
//             body: JSON.stringify({
//                 name: projectName,
//                 sender: sender,
//                 message: message,
//                 timestamp: newMessage.timestamp
//             })
//         });
//     }
// }

// // Helper function to get the current project name
// function getCurrentProjectName() {
//     // Assuming the project name is stored in localStorage or a global variable
//     return localStorage.getItem("currentProjectName");
// }

function formatTimestamp(date) {
    return new Date(date).toLocaleString('en-GB', {
        hour: '2-digit',
        minute: '2-digit',
        day: '2-digit',
        month: 'short',
    }).replace(',', ''); // Remove comma for cleaner format
}

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

    // Save the message to the server and localStorage
    saveMessageToFile(sender, message);
}

function send_message(event) {
    event.preventDefault();
    let messageInput = document.getElementById("input");

    if (messageInput.value.trim() === "") return; // Prevent sending empty messages

    let userMessage = messageInput.value;

    // Save and display user message
    appendMessage('user', userMessage);
    saveMessageToFile("user", userMessage);

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
            saveMessageToFile("bot", response);
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

// Load previous chat messages on page load
function loadMessages() {
    const messages = JSON.parse(localStorage.getItem('chatMessages')) || [];
    messages.forEach(msg => {
        appendMessage(msg.sender, msg.text, msg.timestamp);
    });
}

// if someone can acc work out what this does ill be amazed

// // Function to handle 'Create New Chat' button
// document.getElementById('Create-New').addEventListener('click', () => {
//     const projectName = "guug"; // You can dynamically change this based on the user's selection
//     const chatBoxId = `chatbox-${new Date().getTime()}`; // Unique ID for each new chatbox

//     // Create a new chatbox container
//     const chatContainer = document.createElement('div');
//     chatContainer.id = chatBoxId;
//     chatContainer.classList.add('chat-container');

//     // Create the chat log and append it to the new chatbox
//     const chatlogDiv = document.createElement('div');
//     chatlogDiv.classList.add('chatlog');
//     chatContainer.appendChild(chatlogDiv);

//     // Append the chat container to the body (or a specific parent element)
//     document.body.appendChild(chatContainer);

//     // Set the current project name for the new chat
//     localStorage.setItem("currentProjectName", projectName);

//     // Initialize a new chat for this chatbox
//     appendMessage('bot', 'Welcome! How can I assist you today?', new Date().toLocaleString());
// });

// Initialize chat when the page loads
document.addEventListener("DOMContentLoaded", () => {
    if (window.chatAPI) {
        console.log("chatAPI is available");
        sendWelcomeMessage();
        archie_message();
        loadMessages();
    } else {
        console.error("chatAPI is undefined");
    }
});