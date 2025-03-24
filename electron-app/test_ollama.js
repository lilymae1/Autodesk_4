const axios = require('axios');

async function testModel(input) {
  try {
    const response = await axios.post('http://127.0.0.1:11434', {
      question: input
    }, {
      headers: {
        'Content-Type': 'application/json'
      }
    });

    console.log('Response:', response.data);
  } catch (error) {
    console.error('Error with input', input, ':', error);
  }
}

// Test different inputs
const inputs = [
  "Make all walls 4 meters high",
  "Add a door to the living room",
  "Add a window to the living room",
  "Move the sofa 2 meters to the right",
  "Rotate all chairs by 90 degrees",
  "Delete all trees in the backyard"
];

inputs.forEach(testModel);