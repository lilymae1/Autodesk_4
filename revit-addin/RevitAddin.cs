using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

public class RevitAddin : IExternalCommand
{
    private static string baseProjectPath = "C:\\Users\\joann\\AppData\\Roaming\\RevitChatProjects";

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            
            // Get the project name from the UI (e.g., input form)
            string projectName = "NewProject"; // Dynamically get this from UI
            string projectPath = Path.Combine(baseProjectPath, projectName, projectName + ".rvt");
            
            if (!File.Exists(projectPath))
            {
                // Create a new Revit project if it doesn't exist
                File.Create(projectPath).Close();
            }
            
            Process.Start("C:\\Program Files\\Autodesk\\Revit 2024\\Revit.exe", projectPath);
            
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}