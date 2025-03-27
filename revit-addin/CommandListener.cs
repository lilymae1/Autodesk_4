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

            if (request.HttpMethod == "POST")
            {
                using (var reader = new StreamReader(request.InputStream))
                {
                    string requestBody = await reader.ReadToEndAsync();
                    var commandData = JsonConvert.DeserializeObject<CommandData>(requestBody);

                    if (_uiApp != null)
                    {
                        HandleRevitCommand(_uiApp, commandData.Command, commandData.Parameters);
                    }
                    else
                    {
                        TaskDialog.Show("Error", "UIApplication is null!");
                    }
                }
            }

            context.Response.StatusCode = 200;
            context.Response.Close();
        }
    }

    private static void HandleRevitCommand(UIApplication uiApp, string command, Dictionary<string, object> parameters)
    {
        Document doc = uiApp.ActiveUIDocument.Document;
        TaskDialog.Show("Log", "Processing Revit Command: " + command);

        switch (command)
        {
            case "CreateWall":
                TaskDialog.Show("Log", "Creating wall...");
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
                TaskDialog.Show("Error", "Unknown command received from chatbot.");
                break;
        }
    }

    // Class to deserialize incoming command data
    public class CommandData
    {
        public string Command { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
