using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

public class RevitCommandListener
{
    private static HttpListener _listener = new HttpListener();
    private static UIApplication _uiApp;

    public static void StartListener(UIApplication uiApp)
    {
        if (!_listener.IsListening)
        {
            _uiApp = uiApp;
            TaskDialog.Show("Log", "UIApplication initialized: " + (_uiApp != null ? "Yes" : "No"));
            _listener.Prefixes.Add("http://localhost:5001/revit/");
            _listener.Start();
            Task.Run(() => ListenForCommands());
            TaskDialog.Show("Log", "Listener started");
        }
    }

    public static void StopListener()
    {
        if (_listener.IsListening)
        {
            _listener.Stop();
            _listener.Close();
            TaskDialog.Show("Log", "Listener stopped");
        }
    }


    private static async Task ListenForCommands()
    {
        while (true)
        {
            var context = await _listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            // Initialize the response content to be returned
            string responseMessage = string.Empty;

            if (request.HttpMethod == "POST")
            {
                using (var reader = new StreamReader(request.InputStream))
                {
                    string requestBody = await reader.ReadToEndAsync();
                    Logger.Log(requestBody);
                    var commandData = JsonConvert.DeserializeObject<CommandData>(requestBody);

                    if (_uiApp != null)
                    {
                        try
                        {
                            TaskDialog.Show("Log","test1");
                            Logger.Log(commandData.Command);
                            Logger.Log(string.Join(Environment.NewLine, commandData.Parameters.Select(kv => $"{kv.Key}: {kv.Value}")));
                            HandleRevitCommand(_uiApp, commandData.Command, commandData.Parameters);
                            responseMessage = "Command executed successfully.";
                        }
                        catch (Exception ex)
                        {
                            responseMessage = $"Error processing command: {ex.Message}";
                        }
                    }
                    else
                    {
                        responseMessage = "UIApplication is null!";
                    }
                }
            }
            else
            {
                responseMessage = "Invalid request method. Only POST is supported.";
            }

            // Set the status code based on the outcome
            if (responseMessage.StartsWith("Error"))
            {
                response.StatusCode = 500; // Internal Server Error for errors
            }
            else
            {
                response.StatusCode = 200; // OK for success
            }

            // Write the response message to the response output stream
            using (var writer = new StreamWriter(response.OutputStream))
            {
                writer.Write(responseMessage);
                writer.Flush();
            }

            // Close the response
            response.Close();
        }
    }

    private static void HandleRevitCommand(UIApplication uiApp, string command, Dictionary<string, object> parameters)
    {
        // Ensure that the command is run on Revit's UI thread by calling the UIApplication’s Application
        // and using Revit's mechanisms for running code on the main thread (via transaction).

        // Execute in the Revit application (UI thread context)
        Logger.Log("Processing Revit Command: " + command);

        Document doc = uiApp.ActiveUIDocument.Document;
        if (doc.IsReadOnly)
        {
            Logger.Log("Error: The document is currently read-only and cannot be modified.");
        }


        try
        {
            switch (command)
            {
                case "CreateWall":
                    Logger.Log("Log: Creating wall...");
                    XYZ start = new XYZ(Convert.ToDouble(parameters["startX"]), Convert.ToDouble(parameters["startY"]), 0);
                    XYZ end = new XYZ(Convert.ToDouble(parameters["endX"]), Convert.ToDouble(parameters["endY"]), 0);
                    double height = Convert.ToDouble(parameters["height"]);
                    string wallType = parameters["wallType"].ToString();
                    string level = parameters["level"].ToString();

                    AICommands.CreateWall(doc, start, end, height, wallType, level);
                    break;

                case "ModifyWallHeight":
                    AICommands.ModifyWallHeights(doc, Convert.ToDouble(parameters["newHeight"]));
                    break;

                case "DeleteWall":
                    AICommands.DeleteWall(doc, new ElementId(Convert.ToInt32(parameters["wallId"])));
                    break;

                case "DeleteAllWalls":
                    AICommands.DeleteAllWalls(doc);
                    break;

                default:
                    Logger.Log("Unknown command received from chatbot");
                    TaskDialog.Show("Error", "Unknown command received from chatbot.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Log("Error in command execution: " + ex.Message);
            TaskDialog.Show("Error", "Error in command execution: " + ex.Message);
        }
    }



    // Class to deserialize incoming command data
    public class CommandData
    {
        [JsonProperty("RevitCommand")]
        public string Command { get; set; }

        public Dictionary<string, object> Parameters { get; set; }
    }


    public static class Logger
    {
        private static readonly string logFilePath = @"C:\Users\richa\Documents\Autodesk Project\Autodesk_4\revit-addin\revit_command_log.txt";

        // This method writes a message to the log file
        public static void Log(string message)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(logFilePath, true)) // true to append to the file
                {
                    sw.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                // If logging fails, print to the output window (for debugging purposes)
                Console.WriteLine("Failed to write to log file: " + ex.Message);
            }
        }
    }
}
