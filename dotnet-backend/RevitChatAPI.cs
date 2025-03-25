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
//     private static readonly string BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RevitChatProjects");

//     public RevitChatAPI()
//     {
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
//             System.IO.File.WriteAllText(Path.Combine(projectPath, "info.json"), JsonConvert.SerializeObject(project));
//         }

//         return Ok(new { message = "Project created successfully.", projectPath });
//     }

//     // Endpoint to list all existing projects
//     [HttpGet("projects")]
//     public IActionResult GetProjects()
//     {
//         var projects = Directory.GetDirectories(BaseDirectory)
//             .Select(dir => new
//             {
//                 Name = Path.GetFileName(dir),
//                 Description = LoadProjectDescription(dir),
//                 ChatLogs = Directory.GetFiles(dir, "*.txt").Select(Path.GetFileName).ToList()
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
//         System.IO.File.WriteAllText(newChatPath, "");

//         return Ok(new { message = "New chatbox created.", newChatPath });
//     }

//     // Load project description from its info.json file
//     private static string LoadProjectDescription(string projectPath)
//     {
//         string infoPath = Path.Combine(projectPath, "info.json");
//         if (System.IO.File.Exists(infoPath))
//         {
//             var projectData = JsonConvert.DeserializeObject<ProjectModel>(System.IO.File.ReadAllText(infoPath));
//             return projectData.Description;
//         }
//         return "No description available.";
//     }

//     public class ProjectModel
//     {
//         public string Name { get; set; }
//         public string Description { get; set; }
//     }

//     public class ChatboxRequest
//     {
//         public string ProjectName { get; set; }
//     }
// }

//Updated Version:
//New Project Creation (POST /create-project): 
// It creates a new folder for the project and stores the project details (name, description) in the info.json file.
//Existing Project Listing (GET /projects): 
// It loads all existing projects, reads the project descriptions from the info.json file, and returns them.
// Create Chatbox (POST /create-chatbox): 
// It creates a new chatbox file (Chat_YYYYMMDD_HHMMSS.txt) for the selected project. 
// It also archives the previous chatbox file if one exists.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[ApiController]
[Route("api/chat")]
public class RevitChatAPI : ControllerBase
{
    private static readonly string BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RevitChatProjects");

    public RevitChatAPI()
    {
        if (!Directory.Exists(BaseDirectory))
            Directory.CreateDirectory(BaseDirectory);
    }

    // Endpoint to create a new project
    [HttpPost("create-project")]
    public IActionResult CreateNewProject([FromBody] ProjectModel project)
    {
        if (string.IsNullOrWhiteSpace(project.Name))
            return BadRequest("Project name cannot be empty.");

        string projectPath = Path.Combine(BaseDirectory, project.Name);
        if (!Directory.Exists(projectPath))
        {
            Directory.CreateDirectory(projectPath);
            // Save project info (Name and Description) to info.json
            System.IO.File.WriteAllText(Path.Combine(projectPath, "info.json"), JsonConvert.SerializeObject(project));
        }

        return Ok(new { message = "Project created successfully.", projectPath });
    }

    // Endpoint to list all existing projects
    [HttpGet("projects")]
    public IActionResult GetProjects()
    {
        var projects = Directory.GetDirectories(BaseDirectory)
            .Select(dir => new
            {
                Name = Path.GetFileName(dir),
                Description = LoadProjectDescription(dir),
                ChatLogs = Directory.GetFiles(dir, "*.txt").Select(Path.GetFileName).ToList()
            }).ToList();

        return Ok(projects);
    }

    // Endpoint to create a new chatbox for an existing project
    [HttpPost("create-chatbox")]
    public IActionResult CreateChatbox([FromBody] ChatboxRequest request)
    {
        string projectPath = Path.Combine(BaseDirectory, request.ProjectName);
        if (!Directory.Exists(projectPath))
            return NotFound("Project not found.");

        // Archive previous chatbox file
        var existingChatFiles = Directory.GetFiles(projectPath, "Chat_*.txt").OrderByDescending(f => f).ToList();
        if (existingChatFiles.Any())
        {
            string lastChat = existingChatFiles.First();
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            System.IO.File.Move(lastChat, Path.Combine(projectPath, $"Chat_{timestamp}.txt"));
        }

        // Create a new chatbox file
        string newChatPath = Path.Combine(projectPath, $"Chat_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        System.IO.File.WriteAllText(newChatPath, ""); // Start with an empty file

        return Ok(new { message = "New chatbox created.", newChatPath });
    }

    // Load project description from its info.json file
    private static string LoadProjectDescription(string projectPath)
    {
        string infoPath = Path.Combine(projectPath, "info.json");
        if (System.IO.File.Exists(infoPath))
        {
            var projectData = JsonConvert.DeserializeObject<ProjectModel>(System.IO.File.ReadAllText(infoPath));
            return projectData.Description;
        }
        return "No description available.";
    }

    public class ProjectModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ChatboxRequest
    {
        public string ProjectName { get; set; }
    }
}