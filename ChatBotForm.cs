using CefSharp;
using CefSharp.WinForms;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

public class ChatBotForm : Form
{
    // Declare the browser field at the class level (outside of the constructor and methods)
    private ChromiumWebBrowser browser;

    public ChatBotForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Initialize CefSharp (Only needs to be done once per application)
         if (!Cef.IsInitialized)
        {
            var settings = new CefSettings();
            settings.CefCommandLineArgs.Add("allow-file-access-from-files", "1");
            Cef.Initialize(settings);
        }

        // Form title and size
        this.Text = "Revit Chatbot";
        this.Size = new System.Drawing.Size(500, 700);
        this.AutoScroll = false;

        // Get the directory where the add-in DLL is located
        string dllFolderPath = Directory.GetParent(Directory.GetParent((Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName).FullName).FullName;

        // Get the correct path to chatbot.html
        string htmlFilePath = Path.Combine(dllFolderPath, "chat.html");

        // Ensure the file exists before setting it
        if (File.Exists(htmlFilePath))
        {
            string fileUri = new Uri(htmlFilePath).AbsoluteUri;
            MessageBox.Show($"Loading HTML file from: {fileUri}");

            // Create the Chromium browser and load the HTML file
            browser = new ChromiumWebBrowser(fileUri)
            {
                Dock = DockStyle.Fill
            };

            browser.IsBrowserInitializedChanged += (sender, args) =>
            {
                if (browser.IsBrowserInitialized)
                {
                    browser.ShowDevTools();
                    Console.WriteLine("Browser initialized successfully.");
                }
            };

            this.Controls.Add(browser);
        }
        else
        {
            MessageBox.Show($"Error: chatbot.html not found!\nExpected at: {htmlFilePath}");
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Properly dispose of CefSharp when closing the form
        browser?.Dispose();
        Cef.Shutdown();
        base.OnFormClosing(e);
    }
}
