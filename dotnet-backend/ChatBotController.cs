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
                        // The "response" property contains a string, attempt to parse it as JSON
                        string responseJson = responseElement.GetString();

                        // Ensure the response is valid JSON before parsing
                        if (!string.IsNullOrWhiteSpace(responseJson) && responseJson.Trim().StartsWith("{"))
                        {
                            var responseJsonDoc = JsonDocument.Parse(responseJson);
                            
                            if (responseJsonDoc.RootElement.TryGetProperty("RevitCommand", out JsonElement revitCommandElement))
                            {
                                Console.WriteLine("Running the Revit command...");
                                string command = revitCommandElement.GetString();
                                JsonElement parameters = responseJsonDoc.RootElement.GetProperty("Parameters");

                                var paramDict = JsonSerializer.Deserialize<Dictionary<string, object>>(parameters.ToString());

                                var commandData = new
                                {
                                    RevitCommand = command,
                                    Parameters = paramDict
                                };

                                string commandJson = JsonSerializer.Serialize(commandData);
                                return await ForwardToRevitAPI(commandJson);
                            }
                        }

                        // If it's just a text response, return it as-is
                        return Ok(new { response = responseJson });
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
            var requestContent = new StringContent(revitCommandJson, Encoding.UTF8, "application/json");
            try
            {
                string REVIT_LISTENER_URL = "http://localhost:5001/revit/";
                HttpResponseMessage revitResponse = await _httpClient.PostAsync(REVIT_LISTENER_URL, requestContent);
                
                string revitResponseText = await revitResponse.Content.ReadAsStringAsync();
                Console.WriteLine("Revit Listener Response: " + revitResponseText);

                return Ok(new { response = revitResponseText });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { response = "Failed to communicate with Revit." });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
