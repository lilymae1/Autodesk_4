const ollama = require('ollama').default;

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
    // Log the raw response to understand what we're receiving
    console.log('Response Status:', response.status);
    console.log('Response Headers:', response.headers);

    // Check if the response is JSON before parsing
    const contentType = response.headers.get('Content-Type');
    if (contentType && contentType.includes('application/json')) {
      return response.json(); // Parse as JSON if it's application/json
    } else {
      return response.text(); // Return as text if not JSON
    }
  })
  .then(data => {
    // Log the data (either JSON or text)
    console.log('Response Data:', data);
  })
  .catch(error => {
    console.error('Error:', error); // Log any errors
  });
  
  // TEST
  test('should fetch data successfully', async () => {
    // Use environment variable or default to localhost:3000
    const API_HOST = process.env.OLLAMA_HOST || 'http://127.0.0.1:3000';
    const response = await fetch(`${API_HOST}/api`);
  
    // Log the raw response
    console.log('Response Status:', response.status); 
    console.log('Response Headers:', response.headers);
  
    // Check if the response is JSON before parsing
    const contentType = response.headers.get('Content-Type');
    
    if (contentType && contentType.includes('application/json')) {
      const data = await response.json();
      console.log('Response Data:', data);
    } else {
      console.log('Response is not JSON:', await response.text());
    }
  
    expect(response.status).toBe(200); 
  }); 