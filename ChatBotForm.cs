using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Reflection;
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

            // Get the directory where the add-in DLL is located
            // this is incredibly ineffiecient and needs to be fixed at some point.!!!!!!!
            string dllFolderPath = Directory.GetParent(Directory.GetParent((Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName).FullName).FullName;

            // Get the correct path to chatbot.html
            string htmlFilePath = Path.Combine(dllFolderPath, "chatEnlarged.html");

            // Ensure the file exists before setting it
            if (File.Exists(htmlFilePath))
            {
                MessageBox.Show($"Expected HTML file at: {htmlFilePath}");
                webView.Source = new Uri(htmlFilePath);
            }
            else
            {
                MessageBox.Show($"Error: chatbot.html not found!\nExpected at: {htmlFilePath}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"WebView2 Error: {ex.Message}");
        }
    }
}

