using System;
using Autodesk.Revit.UI;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using RevitChatAPI.Controllers;

namespace RevitChatAddin
{
    public class RevitAddin : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            TaskDialog.Show("Revit Add-In", "Revit Add-In has started!");

            HttpClient client = new HttpClient();
            var response = client.GetStringAsync("http://localhost:3000/api/projects").Result;
            var projects = JsonConvert.DeserializeObject<List<Project>>(response);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication app)
        {
            return Result.Succeeded;
        }
    }
}