using System;
using System.Data.Common;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace RevitChatBotPrototype1
{

    [Transaction(TransactionMode.Manual)]

    // Command that button will perform to open chatbot window
    public class ChatBotCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Start the Electron app
                StartElectronApp.StartElectron();

                

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                message = "Failed to start Electron app: " + ex.Message;
                return Result.Failed;
            }
        }
    }
}
