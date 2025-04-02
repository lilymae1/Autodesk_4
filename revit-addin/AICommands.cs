using Autodesk.Revit.DB;        // For Document, View3D, XYZ, ElementId, etc.
using Autodesk.Revit.UI;        // For TaskDialog, UIApplication, UIDocument
using Autodesk.Revit.Attributes; // For Revit attributes like Transaction
using System;                    // For standard C# types
using System.Collections.Generic; // If you're working with lists
using System.Linq;                // For LINQ methods like FirstOrDefault

public class AICommands
{
    // ----------------------  Wall Modifications ---------------------

    public static void CreateWall(Document doc, XYZ startPoint, XYZ endPoint, double height, string wallType, string level) 
    {
        if (doc == null) return;

        using (Transaction tx = new Transaction(doc, "Create Wall"))
        {
            try
            {
                tx.Start();

                // Find the first available Wall Type (any type)
                FilteredElementCollector wallTypes = new FilteredElementCollector(doc)
                    .OfClass(typeof(WallType));

                WallType selectedWallType = null;

                // Select the first WallType found, assuming at least one exists
                foreach (WallType wt in wallTypes)
                {
                    selectedWallType = wt;
                    break;  // Take the first available Wall Type
                }

                if (selectedWallType == null)
                {
                    TaskDialog.Show("Error", "No Wall Types found in the document.");
                    tx.RollBack();
                    return;
                }

                // Find the Level
                FilteredElementCollector levels = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level));

                Level selectedLevel = null;

                foreach (Level lvl in levels)
                {
                    if (lvl.Name.Equals(level, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedLevel = lvl;
                        break;
                    }
                }

                if (selectedLevel == null)
                {
                    TaskDialog.Show("Error", "Level not found: " + level);
                    tx.RollBack();
                    return;
                }

                // Create a Line for the Wall
                Line wallLine = Line.CreateBound(startPoint, endPoint);

                // Create the Wall using the first available Wall Type and level
                Wall newWall = Wall.Create(doc, wallLine, selectedWallType.Id, selectedLevel.Id, height, 0, false, false);
                tx.Commit();
                TaskDialog.Show("Success", "Wall Created Successfully!");
            }
            catch (Exception ex)
            {
                tx.RollBack();
                TaskDialog.Show("Error", "Failed to create wall: " + ex.Message);
            }
        }
    }




    public static void ModifyWallHeights(Document doc, double newHeight) 
    {

        if (doc == null) return; // Prevent errors if doc is null

        FilteredElementCollector wallCollector = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Walls)
            .WhereElementIsNotElementType();

        using (Transaction tx = new Transaction(doc, "Change All Wall Heights"))
        {
            tx.Start();
            
            foreach (Element elem in wallCollector)
            {
                Wall wall = elem as Wall;
                if (wall != null)
                {
                    Parameter heightParam = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                    if (heightParam != null && !heightParam.IsReadOnly)
                    {
                        heightParam.Set(newHeight);
                    }
                }
            }
            
            tx.Commit();
        }

    }

    public static void DeleteWall(Document doc, ElementId wallId)
    {
        if (doc == null || wallId == null) return; // Prevent errors if doc or wallId is null

        using (Transaction tx = new Transaction(doc, "Delete Wall"))
        {
            tx.Start();

            // Get the wall by its ElementId
            Element wall = doc.GetElement(wallId);

            if (wall != null && wall is Wall)
            {
                // Delete the wall element
                doc.Delete(wallId);
                TaskDialog.Show("Success", "Wall deleted successfully.");
            }
            else
            {
                TaskDialog.Show("Error", "Element is not a valid wall or does not exist.");
            }

            tx.Commit();
        }
    }



    public static void DeleteAllWalls(Document doc)
    {
        if (doc == null) return; // Prevent errors if doc or wallId is null

        FilteredElementCollector wallCollector = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Walls)
            .WhereElementIsNotElementType();

        using (Transaction tx = new Transaction(doc, "Delete All Walls"))
        {
            tx.Start();

            foreach (Element elem in wallCollector) 
            {
                if (elem is Wall wall)
                {
                    // Delete the wall element
                    doc.Delete(elem.Id);
                }
                else
                {
                    TaskDialog.Show("Error", "Element is not a valid wall or does not exist.");
                }

                tx.Commit();
            }
        }
    }

    // ----------------------  Camera Controls ---------------------

    public static void MoveCamera(UIApplication uiApp, XYZ eyePosition, XYZ targetPosition)
    {
        UIDocument uidoc = uiApp.ActiveUIDocument;
        if (uidoc == null)
            return;
        
        Document doc = uidoc.Document;
        View3D view3D = doc.ActiveView as View3D;
        if (doc == null || view3D == null)
            return; // Ensure we have a valid document and 3D view

        using (Transaction tx = new Transaction(doc, "Move Camera"))
        {
            tx.Start();

            // Calculate forward direction (where the camera is looking)
            XYZ forwardDirection = (targetPosition - eyePosition).Normalize();

            // Set a default "Up" direction (assuming Z-axis up)
            XYZ upDirection = XYZ.BasisZ;

            // Create a new ViewOrientation3D with updated eye position
            ViewOrientation3D newOrientation = new ViewOrientation3D(eyePosition, upDirection, forwardDirection);

            // Apply the new orientation to the 3D view
            view3D.SetOrientation(newOrientation);

            tx.Commit();
        }
    }



    public static void ChangeViewType(UIApplication uiApp, string newViewType)
    {
        UIDocument uidoc = uiApp.ActiveUIDocument;
        if (uidoc == null)
            return;

        Document doc = uidoc.Document;
        View view = doc.ActiveView;
        if (view == null)
            return;

        // Attempt to find the new view type's ElementId based on the provided name.
        ElementId newViewTypeId = GetViewTypeId(doc, newViewType);
        if (newViewTypeId == null || newViewTypeId == ElementId.InvalidElementId)
        {
            Logger.Log("Invalid view type specified: " + newViewType);
            return;
        }

        using (Transaction tx = new Transaction(doc, "Change View Type"))
        {
            tx.Start();
            // Change the view's type
            view.ChangeTypeId(newViewTypeId);
            tx.Commit();
        }
    }

    

    public static void RotateCamera(double angleInDegrees, UIApplication uiApp)
    {
        UIDocument uidoc = uiApp.ActiveUIDocument;
        if (uidoc == null)
            return;

        View3D view = uidoc.ActiveView as View3D;
        if (view == null || !view.CanBePrinted)
            return;

        // Get the current camera orientation
        ViewOrientation3D orientation = view.GetOrientation();
        XYZ currentViewDirection = orientation.ForwardDirection;
        XYZ currentUpDirection = orientation.UpDirection;
        XYZ currentCameraPosition = orientation.EyePosition;

        // Define the rotation axis (world Z-axis)
        XYZ rotationAxis = XYZ.BasisZ;
        double angleInRadians = angleInDegrees * (Math.PI / 180);
        
        // Create rotation transformation matrix
        Transform rotationTransform = Transform.CreateRotation(rotationAxis, angleInRadians);

        // Apply rotation to the forward and up vectors
        XYZ newViewDirection = rotationTransform.OfVector(currentViewDirection);
        XYZ newUpDirection = rotationTransform.OfVector(currentUpDirection);

        // Ensure perpendicularity by reconstructing the up direction correctly
        XYZ newRightDirection = newViewDirection.CrossProduct(newUpDirection).Normalize();
        newUpDirection = newRightDirection.CrossProduct(newViewDirection).Normalize();

        // Set the new orientation
        view.SetOrientation(new ViewOrientation3D(newViewDirection, newUpDirection, currentCameraPosition));
    }




    private static ElementId GetViewTypeId(Document doc, string viewTypeName)
    {
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(ViewFamilyType));
        foreach (ViewFamilyType vft in collector)
        {
            if (vft.Name.Equals(viewTypeName, StringComparison.OrdinalIgnoreCase))
            {
                return vft.Id;
            }
        }
        return ElementId.InvalidElementId;
    }

    public static class Logger
    {
        private static readonly string logFilePath = @"C:\Users\richa\Documents\Autodesk Project\Autodesk_4\revit-addin\revit_command_log.txt";

        // This method writes a message to the log file
        public static void Log(string message)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(logFilePath, true)) // true to append to the file
                {
                    sw.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                // If logging fails, print to the output window (for debugging purposes)
                Console.WriteLine("Failed to write to log file: " + ex.Message);
            }
        }
    }

}