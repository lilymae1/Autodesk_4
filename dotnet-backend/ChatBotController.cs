using Microsoft.AspNetCore.Mvc;

namespace AutodeskRevitAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        [HttpPost("getResponse")]
        public IActionResult GetResponse([FromBody] ChatRequest request)
        {
            string aiResponse = GenerateAIResponse(request.Message);
            return Ok(new { response = aiResponse });
        }

        private string GenerateAIResponse(string message)
        {
            // Simulate chatbot logic (replace this with your AI model or logic)
            if (message.ToLower().Contains("hello")) 
                return "Hi! I'm Archie Forklift. How can I assist you today? 😊";
            
            return $"You said: {message}";
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
