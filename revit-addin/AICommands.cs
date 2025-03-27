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

    public static void MoveCamera(Document doc, View3D view3D, XYZ eyePosition, XYZ targetPosition)
    {
        if (doc == null || view3D == null) return; // Prevent errors if doc or view3D is null

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



    public static void ChangeViewType(Document doc, View view, string newViewType)
    {
        if (doc == null || view == null) return;

        using (Transaction tx = new Transaction(doc, "Change View Type"))
        {
            tx.Start();

            // Find the desired view type by name
            ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(vft => vft.Name.Equals(newViewType, StringComparison.OrdinalIgnoreCase));

            if (viewFamilyType != null)
            {
                // Change the view's family type
                view.ChangeTypeId(viewFamilyType.Id);
            }
            else
            {
                TaskDialog.Show("Error", "View Type not found.");
            }

            tx.Commit();
        }
    }

    

    public static void RotateCamera(View3D view, double angleInDegrees)
    {
        if (view == null || !view.CanBePrinted) return; // Ensure it's a 3D view

        // Get the current camera location and view direction
        ViewOrientation3D orientation = view.GetOrientation();
        XYZ currentViewDirection = orientation.ForwardDirection;
        XYZ currentCameraPosition = orientation.EyePosition;

        // Calculate the rotation axis 
        XYZ rotationAxis = XYZ.BasisZ;

        // Convert angle to radians 
        double angleInRadians = angleInDegrees * (Math.PI / 180);

        // Create a rotation matrix around the Z-axis
        Transform rotationTransform = Transform.CreateRotationAtPoint(rotationAxis, angleInRadians, currentCameraPosition);

        // Apply the rotation to the view's orientation
        XYZ newViewDirection = rotationTransform.OfVector(currentViewDirection);
        XYZ newUpDirection = rotationTransform.OfVector(orientation.UpDirection);

        // Set the new orientation for the view
        view.SetOrientation(new ViewOrientation3D(newViewDirection, newUpDirection, currentCameraPosition));
    }


    public static void CreateWindow(Document doc, ElementId wallId, ElementId windowTypeId, XYZ location)
    {
        if (doc == null || wallId == null || windowTypeId == null || location == null) return; // Prevent null errors

        using (Transaction tx = new Transaction(doc, "Create Window"))
        {
            tx.Start();

        // Get the wall by its ElementId
            Wall wall = doc.GetElement(wallId) as Wall;

            if (wall != null)
            {
            // Get the window type
                FamilySymbol windowType = doc.GetElement(windowTypeId) as FamilySymbol;

                if (windowType != null)
                {
                if (!windowType.IsActive) windowType.Activate(); // Ensure the window type is activated

                // Create the window
                    FamilyInstance window = doc.Create.NewFamilyInstance(location, windowType, wall, StructuralType.NonStructural);

                    TaskDialog.Show("Success", "Window created successfully.");
                }
                else
                {
                    TaskDialog.Show("Error", "Invalid window type.");
                }
            }
            else
            {
                TaskDialog.Show("Error", "Wall not found.");
            }

        tx.Commit();
        }
    }

    public static void CreateRoom(Document doc, ElementId levelId, ElementId roomTypeId, XYZ location)
    {
        if (doc == null || levelId == null || roomTypeId == null || location == null) return; // Prevent null errors

        using (Transaction tx = new Transaction(doc, "Create Room"))
        {
            tx.Start();

            // Get the level by its ElementId
            Level level = doc.GetElement(levelId) as Level;

            if (level != null)
            {
            // Get the room type
                RoomType roomType = doc.GetElement(roomTypeId) as RoomType;

                if (roomType != null)
                {
                // Create the room at the specified location on the given level
                    Room room = doc.Create.NewRoom(level, new UV(location.X, location.Y));

                    if (room != null)
                    {
                    // Set the room type (if needed)
                        room.RoomType = roomType;

                        TaskDialog.Show("Success", "Room created successfully.");
                    }
                    else
                    {
                    TaskDialog.Show("Error", "Failed to create room.");
                    }
                }
                else
                {
                    TaskDialog.Show("Error", "Invalid room type.");
                }
            }
            else
            {
                TaskDialog.Show("Error", "Level not found.");
            }

            tx.Commit();
        }
    }

    public static void CreateRoof(Document doc, ElementId levelId, ElementId roofTypeId, CurveArray roofBoundary)
    {
        if (doc == null || levelId == null || roofTypeId == null || roofBoundary == null || roofBoundary.Size == 0) return; // Prevent null or empty boundary errors

        using (Transaction tx = new Transaction(doc, "Create Roof"))
        {
            tx.Start();

            // Get the level by its ElementId
            Level level = doc.GetElement(levelId) as Level;

            if (level != null)
            {
                // Get the roof type
                RoofType roofType = doc.GetElement(roofTypeId) as RoofType;

                if (roofType != null)
                {
                    // Create a roof by extrusion using the specified roof boundary and roof type
                    // Using a Floor or Roof type for the "type" of the roof
                    // RoofBase is the base curve boundary for the roof.

                    // Create a roof instance by extrusion (slope, offset, and type applied)
                    FootPrintRoof newRoof = doc.Create.NewFootPrintRoof(roofBoundary, level, roofType, null);

                    // Optionally, you can apply slope by manipulating the Roof’s Footprint or other properties.

                    TaskDialog.Show("Success", "Roof created successfully.");
                }
                else
                {
                    TaskDialog.Show("Error", "Invalid roof type.");
                }
            }
            else
            {
                TaskDialog.Show("Error", "Level not found.");
            }

            tx.Commit();
        }
    }


}