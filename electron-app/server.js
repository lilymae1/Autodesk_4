const express = require("express");
const fs = require("fs");
const cors = require("cors");
const app = express();

app.use(express.json());
app.use(cors());

const CHAT_LOG_FILE = "chat_log.txt";

// Append message to chat log file
app.post("/saveMessage", (req, res) => {
    const { message } = req.body;
    
    if (!message) {
        return res.status(400).json({ error: "Message is required" });
    }

    const timestamp = new Date().toISOString();
    const logEntry = `${timestamp}: ${message}\n`;

    // Append message to the file
    fs.appendFile(CHAT_LOG_FILE, logEntry, (err) => {
        if (err) {
            return res.status(500).json({ error: "Failed to save message" });
        }
        res.json({ success: true });
    });
});

// Retrieve chat log
app.get("/getChatLog", (req, res) => {
    fs.readFile(CHAT_LOG_FILE, "utf8", (err, data) => {
        if (err) {
            return res.status(500).json({ error: "Failed to load chat log" });
        }
        res.json({ chatLog: data });
    });
});

const PORT = 3000;
app.listen(PORT, () => console.log(`Server running on http://localhost:${PORT}`));
//
