using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RevitChatAddin
{
    // This attribute tells Revit that this is an external application
    [Transaction(TransactionMode.Manual)]
    public class RevitAddin : IExternalApplication
    {
        private const string ApiUrl = "http://localhost:3000/api/projects"; // URL to the API for fetching projects

        // This method is called when Revit starts up
        public Result OnStartup(UIControlledApplication app)
        {
            try
            {
                // Show a message when the add-in starts
                TaskDialog.Show("Revit Add-In", "Revit Add-In has started!");

                // Example: Fetching project data when Revit starts
                GetProjects();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error during startup: {ex.Message}");
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        // This method is called when Revit shuts down
        public Result OnShutdown(UIControlledApplication app)
        {
            // You can add any cleanup code here if needed when Revit closes
            TaskDialog.Show("Revit Add-In", "Revit Add-In is shutting down.");
            return Result.Succeeded;
        }

        // This method fetches project data from the API and displays it in a message box
        private async Task GetProjects()
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30); // Set timeout for the request

            try
            {
                // Make an asynchronous call to the API to get project data
                var response = await client.GetStringAsync(ApiUrl);
                var projects = JsonConvert.DeserializeObject<Project[]>(response);

                // Process the projects (e.g., show in a TaskDialog)
                foreach (var project in projects)
                {
                    TaskDialog.Show("Project", $"Project Name: {project.ProjectName}\nDescription: {project.Description}");
                }
            }
            catch (HttpRequestException e)
            {
                TaskDialog.Show("Error", $"Failed to fetch projects: {e.Message}");
            }
        }
    }

    // A model class to represent a project
    public class Project
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
    }
}