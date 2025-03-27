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
            InitializeEventHandler(uiApp);
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

    private static ExternalEvent _externalEvent;
    private static RevitCommandHandler _commandHandler;

    public static void InitializeEventHandler(UIApplication uiApp)
    {
        if (_commandHandler == null)
        {
            _commandHandler = new RevitCommandHandler(uiApp);
            _externalEvent = ExternalEvent.Create(_commandHandler);
        }
    }

    private static void HandleRevitCommand(UIApplication uiApp, string command, Dictionary<string, object> parameters)
    {
        if (_externalEvent == null || _commandHandler == null)
        {
            Logger.Log("Error: External event handler not initialized.");
            return;
        }

        _commandHandler.SetCommand(command, parameters);
        _externalEvent.Raise();
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
