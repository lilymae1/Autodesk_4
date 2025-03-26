using System;
using System.IO;

namespace RevitChatAddin
{
    public class ChatboxManager
    {
        public void SaveChatMessage(int projectId, string message)
        {
            string projectFolder = Path.Combine("C:", "RevitChatProjects", projectId.ToString());
            string chatFile = Path.Combine(projectFolder, "chatHistory.json");

            if (!File.Exists(chatFile))
            {
                File.WriteAllText(chatFile, "{\"messages\":[]}");
            }

            var chatHistory = JsonConvert.DeserializeObject<ChatHistory>(File.ReadAllText(chatFile));
            chatHistory.Messages.Add(new ChatMessage { Message = message, Timestamp = DateTime.Now });

            File.WriteAllText(chatFile, JsonConvert.SerializeObject(chatHistory));
        }

        public class ChatHistory
        {
            public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        }

        public class ChatMessage
        {
            public string Message { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}