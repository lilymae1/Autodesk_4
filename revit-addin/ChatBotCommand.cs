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

                // Step 2: Simulate getting a response from the chatbot
                //string chatbotResponse = GetChatbotResponse();

                // Step 3: Parse the chatbot response and trigger the corresponding action
                //string command = ParseChatbotResponse(chatbotResponse);

                // Step 4: Handle the parsed command and perform corresponding Revit action
                //HandleRevitCommand(commandData.Application, command);
                
                // Extract thumbnails
                //Thumbnails thumbnailExtractor = new Thumbnails();
                //thumbnailExtractor.ExtractThumbnails(commandData.Application);

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

            switch (command)
            {
                case "CreateWall":
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
    }
}
