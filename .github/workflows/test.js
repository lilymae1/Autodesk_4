const ollama = require('ollama').default;


/// THIS IS FOR THE UNIT TESTS ITS NOT TEMPORARY
/// THIS IS FOR THE UNIT TESTS ITS NOT TEMPORARY
// Define your request data
const inputData = {
  input: 'What is the capital of France?'
};
// Make sure to pass a valid URL and properly structure the fetch request
const url = 'http://127.0.0.1:5000/api/ask.';  // Make sure this is the correct endpoint
 // Ensure the URL is passed as a string

// Use fetch with correct URL and request details
ollama.fetch(url, {
  method: 'POST', // HTTP method
  headers: {
    'Content-Type': 'application/json' // Set header for JSON content
  },
  body: JSON.stringify(inputData) // Convert the data to JSON
})
  .then(response => {
    // Check if the response is JSON before parsing
    const contentType = response.headers.get('Content-Type');
    if (contentType && contentType.includes('application/json')) {
      return response.json(); // Parse as JSON if it's application/json
    } else {
      return response.text(); // Return as text if not JSON
    }
  })
  .then(data => {
  })
  .catch(error => {
  }); 
  
  // TEST
  test('should fetch data successfully', async () => {
    // Use environment variable or default to localhost:5000
    const API_HOST = process.env.OLLAMA_HOST || 'http://127.0.0.1:5000';
    const response = await fetch(`${API_HOST}/api`);

    // Check if the response is JSON before parsing
    const contentType = response.headers.get('Content-Type');

    if (contentType && contentType.includes('application/json')) {
      const data = await response.json();
    } else {
    }
    expect(response.status).toBe(404); 
  }); 