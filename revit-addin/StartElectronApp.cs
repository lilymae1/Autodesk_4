using System;
using System.Diagnostics;
using Autodesk.Revit.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

public class StartElectronApp
{
    public static void StartElectron()
    {
        // Start the C# Web API
        CreateHostBuilder().Build().Start();

        // Start Electron as before
        string nodePath = @"C:\Program Files\nodejs\node.exe";
        string appPath = @"C:\Users\richa\Documents\Autodesk Project\Autodesk_4";

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = nodePath,
                Arguments = $"\"{appPath}\\node_modules\\electron\\dist\\electron.exe\" \"{appPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process electronProcess = new Process { StartInfo = startInfo };
            electronProcess.Start();
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Debug", "Error starting Electron: " + ex.Message);
        }
    }

    // Host the Web API
    public static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://localhost:5000");
                webBuilder.UseStartup<Startup>();
            });
}
