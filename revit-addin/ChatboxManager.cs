// ChatboxManager.cs
using System;
using System.IO;
using Newtonsoft.Json;

public class ChatboxManager
{
    private static string baseChatPath = "C:\\Users\\joann\\AppData\\Roaming\\RevitChatProjects";

    public static void SaveChat(string projectName, string chatContent)
    {
        string chatPath = Path.Combine(baseChatPath, projectName, "chatlog.txt");
        File.AppendAllText(chatPath, chatContent + Environment.NewLine);
    }

    public static string LoadChat(string projectName)
    {
        string chatPath = Path.Combine(baseChatPath, projectName, "chatlog.txt");
        return File.Exists(chatPath) ? File.ReadAllText(chatPath) : "";
    }
}
