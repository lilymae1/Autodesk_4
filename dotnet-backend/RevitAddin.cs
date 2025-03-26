using System;
using Autodesk.Revit.UI;
using System.Net.Http;
using Newtonsoft.Json;

namespace RevitChatAddin
{
    public class RevitAddin : IExternalApplication
    {
        public Result OnStartup(UIApplication app)
        {
            TaskDialog.Show("Revit Add-In", "Revit Add-In has started!");

            HttpClient client = new HttpClient();
            var response = client.GetStringAsync("http://localhost:3000/api/projects").Result;
            var projects = JsonConvert.DeserializeObject<List<Project>>(response);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIApplication app)
        {
            return Result.Succeeded;
        }
    }
}