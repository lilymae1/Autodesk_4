using System;
using System.Diagnostics;
using Autodesk.Revit.UI;

public class StartElectronApp
{
    public static void StartElectron()
    {
        // Path to Node.js (needed to run Electron)
        string nodePath = @"C:\Program Files\nodejs\node.exe";  // Change if Node.js is installed elsewhere
        
        // Path to Electron JS file (or folder with package.json)
        string appPath = @"C:\Users\richa\Documents\Autodesk Project\Autodesk_4";

        try
        {
            // Start the Electron app using Node.js
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = nodePath,  // Use Node.js to run Electron
                Arguments = $"\\node_modules\\electron\\dist\\electron.exe\" \"{appPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process electronProcess = new Process { StartInfo = startInfo };
            electronProcess.Start();

            // Read output for debugging
            string output = electronProcess.StandardOutput.ReadToEnd();
            string error = electronProcess.StandardError.ReadToEnd();
            TaskDialog.Show("Debug", $"Electron Output:\n{output}\nError:\n{error}");
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Debug", "Error starting Electron: " + ex.Message);
        }
    }
}

