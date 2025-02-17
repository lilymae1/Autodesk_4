using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.IO;
using System.Windows.Forms;

// windows form to display chat bot form
public class ChatBotForm : Form
{
    private WebView2 webView;

    public ChatBotForm()
    {
        InitializeComponent();
    }

    private async void InitializeComponent()
    {
        // form title and size
        this.Text = "Revit Chatbot";
        this.Size = new System.Drawing.Size(500, 700);

        webView = new WebView2
        {
            Dock = DockStyle.Fill
        };

        this.Controls.Add(webView);

        try
        {
            // Set a writable custom data directory for WebView2
            string customDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebView2", "RevitChatbot");
            Directory.CreateDirectory(customDataDir); // Ensure the directory exists

            // Create WebView2 environment with the custom directory
            var env = await CoreWebView2Environment.CreateAsync(null, customDataDir);
            await webView.EnsureCoreWebView2Async(env);

            // Load chatbot UI using local html file
            // This may need to be changed to sync with out system
            webView.Source = new Uri("C:\\Users\\richa\\source\\repos\\RevitChatBotPrototype1\\RevitChatBotPrototype1\\chatbot.html");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"WebView2 Error: {ex.Message}");
        }
    }
}