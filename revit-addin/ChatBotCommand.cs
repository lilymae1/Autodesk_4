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
                string chatbotResponse = GetChatbotResponse();

                // Step 3: Parse the chatbot response and trigger the corresponding action
                string command = ParseChatbotResponse(chatbotResponse);

                // Step 4: Handle the parsed command and perform corresponding Revit action
                HandleRevitCommand(commandData.Application, command);
                
                // Extract thumbnails
                Thumbnails thumbnailExtractor = new Thumbnails();
                thumbnailExtractor.ExtractThumbnails(commandData.Application);

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
        private void HandleRevitCommand(UIApplication uiApp, string command)
        {
            Document doc = uiApp.ActiveUIDocument.Document;

            // You can extend the switch case to add more commands as needed
            switch (command)
            {
                case "CreateWall":
                    // Example parameters for wall creation, this can be customized
                    AICommands.CreateWall(doc, new XYZ(0, 0, 0), new XYZ(10, 0, 0), 10.0, "Basic Wall", "Level 1");
                    break;

                case "ModifyWallHeight":
                    // Example to modify all wall heights, e.g., changing height to 12.0
                    AICommands.ModifyWallHeights(doc, 12.0);
                    break;

                case "ChangeWallWidth":
                    // Example to change all wall widths, e.g., to 0.5
                    AICommands.ChangeWallWidth(doc, 0.5);
                    break;

                case "DeleteWall":
                    // Example to delete a specific wall (this would typically need an ID or parameters)
                    ElementId wallId = new ElementId(12345);  // Placeholder for the wall ID
                    AICommands.DeleteWall(doc, wallId);
                    break;

                case "DeleteAllWalls":
                    // Delete all walls in the document
                    AICommands.DeleteAllWalls(doc);
                    break;

                default:
                    TaskDialog.Show("Error", "Unknown command received from chatbot.");
                    break;
            }
        }
    }
}
