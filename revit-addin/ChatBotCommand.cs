using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitChatBotPrototype1
{
    [Transaction(TransactionMode.Manual)]
    // Command that button will perform to open chatbot window and handle Revit model changes
    public class ChatBotCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Step 1: Start the Electron app to open the chatbot
                StartElectronApp.StartElectron();

                string requestUri = "http://localhost:5000/api/revit/execute"; // Revit API endpoint

                // Wait for incoming HTTP request for Revit command
                HttpResponseMessage response = client.GetAsync(requestUri).Result; // Get response from the backend API

                if (response.IsSuccessStatusCode)
                {
                    // Parse the response from Revit API
                    string responseJson = response.Content.ReadAsStringAsync().Result;
                    var commandData = JsonConvert.DeserializeObject<CommandData>(responseJson);

                    // Now execute the command in Revit
                    HandleRevitCommand(commandData.Command, commandData.Parameters);
                }
                else
                {
                    message = "Failed to execute Revit command.";
                    return Result.Failed;
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                message = "Failed to execute command: " + ex.Message;
                message = "Failed to start Electron app or extract thumbnails: " + ex.Message;
                return Result.Failed;
            }
        }

        // Step 1: Simulate receiving a response from the chatbot (in a real scenario, this would be dynamic)
        private string GetChatbotResponse()
        {
            // For this example, the chatbot response is hardcoded.
            // In a real-world scenario, this would come from the Electron app.
            return "CreateWall";
        }

        // Step 2: Parse the chatbot response to determine the command
        private string ParseChatbotResponse(string chatbotResponse)
        {
            // Here you can implement parsing to differentiate between commands
            // For simplicity, we'll use the response directly.
            return chatbotResponse;
        }

        // Step 3: Handle the parsed command and call the appropriate function from AICommands
        private void HandleRevitCommand(UIApplication uiApp, string command, Dictionary<string, object> parameters)
        {
            Document doc = uiApp.ActiveUIDocument.Document;
            TaskDialog.Show("Log", "before swtich");

            switch (command)
            {
                case "CreateWall":
                    TaskDialog.Show("Log", "chose create wall");
                    XYZ start = new XYZ(Convert.ToDouble(parameters["startX"]), Convert.ToDouble(parameters["startY"]), 0);
                    XYZ end = new XYZ(Convert.ToDouble(parameters["endX"]), Convert.ToDouble(parameters["endY"]), 0);
                    double height = Convert.ToDouble(parameters["height"]);
                    string wallType = parameters["wallType"].ToString();
                    string level = parameters["level"].ToString();
                    TaskDialog.Show("Log", "attempt to create wall");
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
}
