const axios = require('axios');

/**
 * Send a structured prompt to Ollama and get JSON output.
 * @param {string} prompt - The user input.
 * @returns {Promise<Object>} - Parsed JSON response.
 */
async function queryOllama(prompt) {
    try {
        const response = await axios.post('http://localhost:11434/api/generate', {
            model: "openhermes:7b-mistral-v2.5-q4_0",
            prompt: prompt,
            format: "json", 
            stream: false,
        });

        const aiResponse = response.data.response;
        console.log("Raw AI Response:", aiResponse);

        
        try {
            const parsedJson = JSON.parse(aiResponse);
            return parsedJson;
        } catch (jsonError) {
            console.error("Error parsing JSON:", jsonError);
            return { error: "Invalid JSON format from AI" };
        }
    } catch (error) {
        console.error("Error calling Ollama:", error);
        return { error: "AI request failed" };
    }
}

module.exports = { queryOllama };