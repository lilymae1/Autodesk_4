using System;
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
        string saveFolder = @".\electron-app\UI\Assets\ChatThumbnails";

        // get files
        string projectFolder = @"C:\Program Files\Autodesk\Revit 2025\Samples";
        string[] files = Directory.GetFiles(projectFolder, "*.rfa"); 

        // for every file
        foreach(string project in files) {

            // get the name
            string name = Path.GetFileNameWithoutExtension(project);
            
            // Load the Revit file (Document)
            Document doc = uiapp.Application.OpenDocumentFile(project);
            
            // Get a view to extract the thumbnail
            View view = GetFirstView(doc);

            // Path to save the image
            string savePath = Path.Combine(saveFolder, name + ".png");

            // Export the view to an image file
            ExportViewToImage(doc, view, savePath);
        }
    }

    private View GetFirstView(Document doc)
    {
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(View));

        // Get the first view in the document
        return collector.FirstElement() as View;
    }

    private void ExportViewToImage(Document doc, View view, string filePath)
    {
        // Set up the export options
        ImageExportOptions options = new ImageExportOptions
        {
            
            FilePath = filePath,
            ExportRange = ExportRange.VisibleRegionOfCurrentView,
            ImageResolution = (ImageResolution)72,
            ZoomType = ZoomFitType.FitToPage, 
            PixelSize = 200  
            
        };

        // Export the view to an image
        doc.ExportImage(options);
    }
}