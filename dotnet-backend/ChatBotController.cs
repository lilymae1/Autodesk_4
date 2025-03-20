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
            Console.WriteLine($"Received message: {request?.Message}"); // Log the received message

            if (request?.Message == null)
            {
                return BadRequest(new { response = "Error: Message was null or empty." });
            }

            var response = $"You said: {request.Message}";
            Console.WriteLine($"Sending response: {response}"); // Log the response
            return Ok(new { response });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
