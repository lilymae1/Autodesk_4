// using System;
// using System.Diagnostics;
// using Autodesk.Revit.UI;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.Extensions.Hosting;

// public class StartElectronApp
// {
//     public static async void StartElectron()
//     {
//         string nodePath = @"C:\Program Files\nodejs\node.exe";
//         string electronPath = @"C:\Users\richa\Documents\Autodesk Project\Autodesk_4\electron-app\node_modules\electron\dist\electron.exe";
//         string appPath = @"C:\Users\richa\Documents\Autodesk Project\Autodesk_4\electron-app";

//         try
//         {
//             ProcessStartInfo startInfo = new ProcessStartInfo
//             {
//                 FileName = electronPath,  // Direct path to electron.exe
//                 Arguments = $"\"{appPath}\"",
//                 WorkingDirectory = appPath,
//                 UseShellExecute = false,
//                 RedirectStandardOutput = true, // Capture console output for debugging
//                 RedirectStandardError = true,  // Capture error messages for clarity
//                 CreateNoWindow = false
//             };

//             Process electronProcess = new Process { StartInfo = startInfo };

//             electronProcess.OutputDataReceived += (sender, e) => 
//                 TaskDialog.Show("Electron Log", e.Data ?? "No output");
//             electronProcess.ErrorDataReceived += (sender, e) => 
//                 TaskDialog.Show("Electron Error", e.Data ?? "No error");

//             electronProcess.Start();

//              // Read the output asynchronously
//             string output = await electronProcess.StandardOutput.ReadToEndAsync();
//             string error = await electronProcess.StandardError.ReadToEndAsync();
//             // Write logs to a file for easier tracking
//             string logPath = Path.Combine(appPath, "revit-electron-log.txt");
//             File.WriteAllText(logPath, $"Output:\n{output}\n\nError:\n{error}");

//             TaskDialog.Show("Electron", "Electron app started successfully!");
//         }
//         catch (Exception ex)
//         {
//             TaskDialog.Show("Debug", "Error starting Electron: " + ex.Message);
//         }
//     }
//     public static void LogMessage(string message)
//     {   
//         string logPath = @"C:\Users\richa\Documents\Autodesk Project\Autodesk_4\revit-backend-log.txt";
//         using (StreamWriter writer = new StreamWriter(logPath, true))
//         {
//             writer.WriteLine($"{DateTime.Now}: {message}");
//         }
//     }
// }

using System;
using System.Diagnostics;
using Autodesk.Revit.UI;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;

public class StartElectronApp
{
    public static async void StartElectron()
    {
        // Get the directory where the add-in DLL is located
        string addinDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        // Move two directory up
        string projectRoot = Path.GetFullPath(Path.Combine(addinDirectory, "..","..","..",".."));
        
        // Construct Electron paths
        string electronPath = Path.Combine(projectRoot, "electron-app", "node_modules", "electron", "dist", "electron.exe");
        string appPath = Path.Combine(projectRoot, "electron-app");

        TaskDialog.Show("Electron Log", $"Add-in Directory: {addinDirectory}");
        TaskDialog.Show("Electron Log", $"Project Root (Parent Directory): {projectRoot}");
        TaskDialog.Show("Electron Log", $"Electron Path: {electronPath}");
        TaskDialog.Show("Electron Log",$"App Path: {appPath}");

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = electronPath,  // Direct path to electron.exe
                Arguments = $"\"{appPath}\"",
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

             // Read the output asynchronously
            string output = await electronProcess.StandardOutput.ReadToEndAsync();
            string error = await electronProcess.StandardError.ReadToEndAsync();
            // Write logs to a file for easier tracking
            string logPath = Path.Combine(appPath, "revit-electron-log.txt");
            File.WriteAllText(logPath, $"Output:\n{output}\n\nError:\n{error}");

            TaskDialog.Show("Electron", "Electron app started successfully!");
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Debug", "Error starting Electron: " + ex.Message);
        }
    }

    public static void LogMessage(string message)
    {   
        string logPath = @"C:\Users\richa\Documents\Autodesk Project\Autodesk_4\revit-backend-log.txt";
        using (StreamWriter writer = new StreamWriter(logPath, true))
        {
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}

