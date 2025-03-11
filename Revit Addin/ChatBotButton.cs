using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace RevitChatBotPrototype1
{
    // Button to appear in Revit RibbonPanel (top of UI)
    internal class ChatBotButton : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panel = application.CreateRibbonPanel("ChatBot Panel");

            // Text / data for button
            PushButtonData button = new PushButtonData(
                "Archie Forklift",
                "Open ChatBot",
                System.Reflection.Assembly.GetExecutingAssembly().Location,
                "RevitChatBotPrototype1.ChatBotCommand"
                );

            panel.AddItem(button);

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
