using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace AutodeskRevitAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        // POST method to get a response from the chatbot model
        [HttpPost("getResponse")]
        public async Task<IActionResult> GetResponse([FromBody] ChatRequest request)
        {
            // Log the incoming request message for debugging
            Console.WriteLine($"Received request: {request?.Message}");

            // Check if the message is empty or null
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return BadRequest(new { response = "Error: Message was null or empty." });
            }

            // Assuming you want to interact with Ollama or another model:
            string url = "http://localhost:11434/api/generate"; // Ollama API endpoint
            string model = "revit/archiemodel"; // Change this to any model you want (like llama3, gemma, etc.)
            
            using (HttpClient client = new HttpClient())
            {
                var requestData = new
                {
                    model = model,
                    prompt = request.Message, // Pass the message as the prompt
                    stream = false  // Set to 'true' if you want streaming responses
                };

                string jsonRequest = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                try
                {
                    // Send the request to Ollama API
                    HttpResponseMessage apiResponse = await client.PostAsync(url, content);

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        string jsonResponse = await apiResponse.Content.ReadAsStringAsync();
                        var jsonDoc = JsonDocument.Parse(jsonResponse);

                        // Make sure you're getting the AI's response, not the input
                        if (jsonDoc.RootElement.TryGetProperty("response", out var responseElement))
                        {
                            string responseText = responseElement.GetString();
                            return Ok(new { response = responseText });
                        }
                        else
                        {
                            return BadRequest("Invalid API response format.");
                        }
                    }
                    else
                    {
                        return StatusCode((int)apiResponse.StatusCode, new { response = "Error communicating with Ollama API." });
                    }
                }
                catch (Exception ex)
                {
                    // Log and return an internal error if something goes wrong
                    Console.WriteLine($"Error: {ex.Message}");
                    return StatusCode(500, new { response = "Internal server error." });
                }
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
