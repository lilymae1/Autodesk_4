using System;
using System.IO;
using Newtonsoft.Json;

namespace RevitChatAddin
{
    public class ChatboxManager
    {
        public void SaveChatMessage(int projectId, string message)
        {
            // Path for the project directory
            string projectFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RevitChatProjects", projectId.ToString());
            string chatFile = Path.Combine(projectFolder, "chatHistory.json");

            if (!File.Exists(chatFile))
            {
                // Initialize a new chat history if it doesn't exist
                File.WriteAllText(chatFile, "{\"messages\":[]}");
            }

            // Read the chat history and append the new message
            var chatHistory = JsonConvert.DeserializeObject<ChatHistory>(File.ReadAllText(chatFile));
            chatHistory.Messages.Add(new ChatMessage { Message = message, Timestamp = DateTime.Now });

            // Save the updated chat history back to the file
            File.WriteAllText(chatFile, JsonConvert.SerializeObject(chatHistory));
        }

        // Model class for chat history
        public class ChatHistory
        {
            public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        }

        // Model class for each chat message
        public class ChatMessage
        {
            public string Message { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}