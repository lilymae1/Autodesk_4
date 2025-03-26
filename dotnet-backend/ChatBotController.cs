using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;

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
            _httpClient.Timeout = new TimeSpan(0, 5, 0); //sets timeout

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
                    Console.WriteLine(jsonResponse);

                    // Check if the response is a structured command (RevitCommand)
                    if (jsonDoc.RootElement.TryGetProperty("response", out JsonElement responseElement))
                    {
                        // The "response" property contains a stringified JSON
                        string responseJson = responseElement.GetString();

                        // Deserialize the stringified JSON to a JsonDocument
                        var responseJsonDoc = JsonDocument.Parse(responseJson);

                        // Now you can safely check for the "RevitCommand" in the nested JSON
                        if (responseJsonDoc.RootElement.TryGetProperty("RevitCommand", out JsonElement revitCommandElement))
                        {
                            Console.WriteLine("Running the Revit command...");
                            // Forward the detected Revit command to the Revit API
                            string command = revitCommandElement.GetString();
                            JsonElement parameters = responseJsonDoc.RootElement.GetProperty("Parameters");

                            // Convert parameters to a dictionary (or use as needed)
                            var paramDict = JsonSerializer.Deserialize<Dictionary<string, object>>(parameters.ToString());

                            // Serialize both command and parameters into one JSON object
                            var commandData = new
                            {
                                RevitCommand = command,
                                Parameters = paramDict
                            };
                            string commandJson = JsonSerializer.Serialize(commandData);

                            // Send the command to the Revit API
                            return await ForwardToRevitAPI(commandJson);
                        }
                        else
                        {
                            return BadRequest("RevitCommand not found in the response.");
                        }
                    }
                    else
                    {
                        return BadRequest("No response property found in the JSON.");
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
            // Send the serialized JSON (which contains both command and parameters) to the Revit API
            var requestContent = new StringContent(revitCommandJson, Encoding.UTF8, "application/json");
            HttpResponseMessage revitResponse = await _httpClient.PostAsync(REVIT_API_URL, requestContent);

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
