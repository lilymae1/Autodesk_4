const ollama = require('ollama');
const fs = require('fs');

// Define test data
const testData = [
    { "input": "Make all walls 4 meters high", "output": "modify_walls(height=4.0)" },
    { "input": "Add a door to the living room", "output": "add_door(room='Living Room')" },
    // More test cases...
];

// Function to test the AI model
async function testModel(prompt, expectedOutput) {
    try {
        // Assuming 'ollama.generate()' is the correct function
        const response = await ollama.generate({ prompt: prompt });

        const result = response.text;  // Assuming the response contains the text field

        // Check if the result matches the expected output
        if (result === expectedOutput) {
            console.log(`Test Passed: ${prompt}`);
        } else {
            console.log(`Test Failed: ${prompt}`);
            console.log(`Expected: ${expectedOutput}`);
            console.log(`Got: ${result}`);
        }
    } catch (error) {
        console.error(`Error with input "${prompt}":`, error);
    }
}

// Loop through test data and run tests asynchronously
testData.forEach(async (testCase) => {
    await testModel(testCase.input, testCase.output);
});

// Test specific inputs (if necessary)
const testInputs = [
    "Make all walls 4 meters high",
    "Add a window to the living room",
    "Move the sofa 2 meters to the right",
    "Rotate all chairs by 90 degrees",
    "Delete all trees in the backyard"
];

testInputs.forEach(async (input) => {
    await testModel(input, "Expected Output for this input");  // Replace with actual expected output
});
