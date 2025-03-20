using System;
using System.Diagnostics;
using Autodesk.Revit.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

public class StartElectronApp
{
    public static void StartElectron()
    {
        string nodePath = @"C:\Program Files\nodejs\node.exe";
        string appPath = @"C:\Users\richa\Documents\Autodesk Project\Autodesk_4\electron-app";

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = nodePath,
                Arguments = $"\"{appPath}\\node_modules\\.bin\\electron.cmd\" \"{appPath}\"",
                WorkingDirectory = appPath,
                UseShellExecute = false,
                RedirectStandardOutput = true, // Capture console output for debugging
                RedirectStandardError = true,  // Capture error messages for clarity
                CreateNoWindow = false
            };

            Process electronProcess = new Process { StartInfo = startInfo };

            electronProcess.OutputDataReceived += (sender, e) => 
                TaskDialog.Show("Electron Log", e.Data ?? "No output");
            electronProcess.ErrorDataReceived += (sender, e) => 
                TaskDialog.Show("Electron Error", e.Data ?? "No error");

            electronProcess.Start();
            electronProcess.BeginOutputReadLine();
            electronProcess.BeginErrorReadLine();

            TaskDialog.Show("Electron", "Electron app started successfully!");
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Debug", "Error starting Electron: " + ex.Message);
        }
    }
}

