using Microsoft.AspNetCore.Mvc;
using System;
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
        private HttpClient _httpClient = new HttpClient();
        private const string OLLAMA_API_URL = "http://localhost:11434/api/generate"; // Ollama API endpoint
        private const string REVIT_API_URL = "http://localhost:5000/api/revit/execute"; // Revit API endpoint

        [HttpPost("getResponse")]
        public async Task<IActionResult> GetResponse([FromBody] ChatRequest request)
        {
            _httpClient.Timeout = new TimeSpan(0,5,0);//sets timeout (hours,mins,secs) not sure if this has worked but we movin

            Console.WriteLine($"Received request: {request?.Message}");

            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return BadRequest(new { response = "Error: Message was null or empty." });
            }

            var requestData = new
            {
                model = "revit/archiemodel",
                prompt = request.Message,
                stream = false
            };

            string jsonRequest = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                // Send request to Ollama API
                HttpResponseMessage apiResponse = await _httpClient.PostAsync(OLLAMA_API_URL, content);
                if (apiResponse.IsSuccessStatusCode)
                {
                    string jsonResponse = await apiResponse.Content.ReadAsStringAsync();  // Retrieve the response from Ollama
                    var jsonDoc = JsonDocument.Parse(jsonResponse);

                    // Check if the response is a structured command
                    if (jsonDoc.RootElement.TryGetProperty("RevitCommand", out JsonElement revitCommandElement))
                    {
                        Console.WriteLine("Detected Revit Command! Forwarding to Revit API...");
                        return await ForwardToRevitAPI(jsonResponse);  // Forward the command to Revit API
                    }
                    else
                    {
                        // Return plain text response if no Revit command
                        if (jsonDoc.RootElement.TryGetProperty("response", out var responseElement))
                        {
                            string responseText = responseElement.GetString();
                            return Ok(new { response = responseText });
                        }
                        else
                        {
                            return BadRequest("Invalid response format from Ollama.");
                        }
                    }
                }
                else
                {
                    return StatusCode((int)apiResponse.StatusCode, new { response = "Error communicating with Ollama API." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { response = "Internal server error." });
            }
        }

        private async Task<IActionResult> ForwardToRevitAPI(string revitCommandJson)
        {
            var content = new StringContent(revitCommandJson, Encoding.UTF8, "application/json");
            HttpResponseMessage revitResponse = await _httpClient.PostAsync(REVIT_API_URL, content);

            if (!revitResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)revitResponse.StatusCode, new { response = "Error executing Revit command." });
            }

            string revitResponseText = await revitResponse.Content.ReadAsStringAsync();
            return Ok(new { response = revitResponseText });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
