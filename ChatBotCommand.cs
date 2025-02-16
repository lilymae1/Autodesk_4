using System;
using System.Data.Common;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Web.WebView2.WinForms;

namespace RevitChatBotPrototype1
{

    [Transaction(TransactionMode.Manual)]

    // Command that button will perform to open chatbot window
    public class ChatBotCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // attempt to make windows form
            try
            {
                ChatBotForm chatForm = new ChatBotForm();
                chatForm.Show();  // Opens the chatbot window
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return Result.Failed;
            }
        }
    }
}
