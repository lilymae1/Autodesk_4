using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Drawing;
using System.Drawing.Imaging;

[ApiController]

public class Thumbnails
{
    public void ExtractThumbnails(UIApplication uiapp)
    {
        // folder inwhich thumbnails are saved
        string saveFolder = Path.GetFullPath(@".\electron-app\UI\Assets\ChatThumbnails");
        Directory.CreateDirectory(saveFolder);

        // get files
        string projectFolder = @"C:\Program Files\Autodesk\Revit 2025\Samples";
        string[] files = Directory.GetFiles(projectFolder, "*.rfa"); 

        // for every file
        foreach(string project in files) {

            Console.WriteLine("checking file:" + project);
            // get the name
            string name = Path.GetFileNameWithoutExtension(project);
            
            // Load the Revit file (Document)
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(project);
            OpenOptions openOptions = new OpenOptions();
            Document doc = uiapp.Application.OpenDocumentFile(modelPath, openOptions);
            
            // Get a view to extract the thumbnail
            View view = GetFirstView(doc);
            if (view == null)
            {
                Console.WriteLine("No valid view found, skipping...");
                continue;
            }

            // Path to save the image
            string savePath = Path.Combine(saveFolder, name + ".png");
            Console.WriteLine("Saving image " + savePath);

            // Export the view to an image file
            ExportViewToImage(doc, view, savePath);
            doc.Close(false);
        }
    }

    private View GetFirstView(Document doc)
    {
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(View));

        Console.WriteLine("Returning first view");
        // Get the first view in the document
        return collector.FirstElement() as View;
    }

    private void ExportViewToImage(Document doc, View view, string filePath)
    {
        // Set up the export options
        ImageExportOptions options = new ImageExportOptions();
            
        options.HLRandWFViewsFileType = ImageFileType.PNG;
        options.PixelSize = 200;
        options.ExportRange = ExportRange.VisibleRegionOfCurrentView;
            
        Console.WriteLine("Exporting image:" + filePath);
        // Export the view to an image
        doc.ExportImage(options);
    }
}