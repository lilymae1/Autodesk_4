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

                RevitCommandListener.StartListener(commandData.Application);

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
    }
}
