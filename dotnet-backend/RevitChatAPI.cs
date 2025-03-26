using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;

namespace RevitChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        // POST method to create a new project
        [HttpPost]
        public IActionResult CreateProject([FromBody] Project project)
        {
            // Define the directory path where the project will be stored
            string projectDirectory = Path.Combine("C:", "RevitChatProjects", project.ProjectId.ToString());

            // Check if the project directory already exists
            if (Directory.Exists(projectDirectory))
            {
                return BadRequest("Project already exists.");
            }

            // Create the project directory
            Directory.CreateDirectory(projectDirectory);

            // Define the path for the project JSON file
            string projectFile = Path.Combine(projectDirectory, "project.json");

            // Write the project data to a JSON file using System.IO.File.WriteAllText
            System.IO.File.WriteAllText(projectFile, JsonConvert.SerializeObject(project));

            // Return the created project as a response
            return CreatedAtAction(nameof(GetProjectById), new { id = project.ProjectId }, project);
        }

        // GET method to retrieve a project by its ID
        [HttpGet("{id}")]
        public IActionResult GetProjectById(int id)
        {
            // Define the path to the project JSON file
            string projectFile = Path.Combine("C:", "RevitChatProjects", id.ToString(), "project.json");

            // Check if the project file exists
            if (!System.IO.File.Exists(projectFile))
            {
                return NotFound();
            }

            // Read the project data from the JSON file and deserialize it
            var project = JsonConvert.DeserializeObject<Project>(System.IO.File.ReadAllText(projectFile));

            // Return the project data as a response
            return Ok(project);
        }
    }

    // Project model class to represent project data
    public class Project
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
    }
}

// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using Microsoft.AspNetCore.Mvc;
// using Newtonsoft.Json;

// [ApiController]
// [Route("api/chat")]
// public class RevitChatAPI : ControllerBase
// {
//     // Define the base directory for project storage
//     private static readonly string BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Roaming", "Autodesk", "Revit", "Addins", "ChatProjects");

//     public RevitChatAPI()
//     {
//         // Ensure the directory exists for saving projects
//         if (!Directory.Exists(BaseDirectory))
//             Directory.CreateDirectory(BaseDirectory);
//     }

//     // Endpoint to create a new project
//     [HttpPost("create-project")]
//     public IActionResult CreateNewProject([FromBody] ProjectModel project)
//     {
//         if (string.IsNullOrWhiteSpace(project.Name))
//             return BadRequest("Project name cannot be empty.");

//         string projectPath = Path.Combine(BaseDirectory, project.Name);
//         if (!Directory.Exists(projectPath))
//         {
//             Directory.CreateDirectory(projectPath);
//             // Save project info (Name and Description) to info.json
//             System.IO.File.WriteAllText(Path.Combine(projectPath, "info.json"), JsonConvert.SerializeObject(project));
//         }

//         return Ok(new { message = "Project created successfully.", projectPath });
//     }

//     // Endpoint to list all existing projects
//     [HttpGet("projects")]
//     public IActionResult GetProjects()
//     {
//         var projects = Directory.GetDirectories(BaseDirectory)
//             .Select(dir =>
//             {
//                 string description = LoadProjectDescription(dir);
//                 return new
//                 {
//                     Name = Path.GetFileName(dir),
//                     Description = description,
//                     ChatLogs = Directory.GetFiles(dir, "*.txt").Select(Path.GetFileName).ToList()
//                 };
//             }).ToList();

//         return Ok(projects);
//     }

//     // Endpoint to create a new chatbox for an existing project
//     [HttpPost("create-chatbox")]
//     public IActionResult CreateChatbox([FromBody] ChatboxRequest request)
//     {
//         string projectPath = Path.Combine(BaseDirectory, request.ProjectName);
//         if (!Directory.Exists(projectPath))
//             return NotFound("Project not found.");

//         // Archive previous chatbox file
//         var existingChatFiles = Directory.GetFiles(projectPath, "Chat_*.txt").OrderByDescending(f => f).ToList();
//         if (existingChatFiles.Any())
//         {
//             string lastChat = existingChatFiles.First();
//             string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
//             System.IO.File.Move(lastChat, Path.Combine(projectPath, $"Chat_{timestamp}.txt"));
//         }

//         // Create a new chatbox file
//         string newChatPath = Path.Combine(projectPath, $"Chat_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
//         System.IO.File.WriteAllText(newChatPath, ""); // Start with an empty file

//         return Ok(new { message = "New chatbox created.", newChatPath });
//     }

//     // Endpoint to update project description
//     [HttpPost("update-project")]
//     public IActionResult UpdateProject([FromBody] ProjectModel project)
//     {
//         if (string.IsNullOrWhiteSpace(project.Name))
//             return BadRequest("Project name cannot be empty.");

//         string projectPath = Path.Combine(BaseDirectory, project.Name);
//         if (!Directory.Exists(projectPath))
//             return NotFound("Project not found.");

//         // Load and update project info
//         string infoPath = Path.Combine(projectPath, "info.json");
//         if (!System.IO.File.Exists(infoPath))
//             return NotFound("Project info file not found.");

//         try
//         {
//             var existingProject = JsonConvert.DeserializeObject<ProjectModel>(System.IO.File.ReadAllText(infoPath));
//             existingProject.Description = project.Description;

//             // Save updated description to the info.json file
//             System.IO.File.WriteAllText(infoPath, JsonConvert.SerializeObject(existingProject));

//             return Ok(new { message = "Project updated successfully." });
//         }
//         catch (Exception)
//         {
//             return StatusCode(500, "Error updating project.");
//         }
//     }

//     // Load project description from its info.json file
//     private static string LoadProjectDescription(string projectPath)
//     {
//         string infoPath = Path.Combine(projectPath, "info.json");
//         if (System.IO.File.Exists(infoPath))
//         {
//             try
//             {
//                 var projectData = JsonConvert.DeserializeObject<ProjectModel>(System.IO.File.ReadAllText(infoPath));
//                 return projectData?.Description ?? "No description available."; // Safe check for null Description
//             }
//             catch (Exception)
//             {
//                 return "Error reading project info."; // Handle deserialization errors
//             }
//         }
//         return "No description available.";
//     }

//     // Project model for request/response
//     public class ProjectModel
//     {
//         public string Name { get; set; }
//         public string Description { get; set; }
//     }

//     // Request model for creating a chatbox
//     public class ChatboxRequest
//     {
//         public string ProjectName { get; set; }
//     }
// }