using System;
using System.Diagnostics;
using Autodesk.Revit.UI;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;

public class AICommands
{
    // ----------------------  Wall Modifications ---------------------

    public static void CreateWall(Document doc, 
                                  XYZ startPoint = null, 
                                  XYZ endPoint = null, 
                                  double height = 10.0, 
                                  string wallTypeName = "Basic Wall", 
                                  string levelName = "Level 1") 
    {
        if (doc == null) return;

        using (Transaction tx = new Transaction(doc, "Create Wall"))
        {
            tx.Start();

            // Find the Wall Type
            FilteredElementCollector wallTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(WallType));

            WallType selectedWallType = null;
            foreach (WallType wt in wallTypes)
            {
                if (wt.Name.Equals(wallTypeName, StringComparison.OrdinalIgnoreCase))
                {
                    selectedWallType = wt;
                    break;
                }
            }
            if (selectedWallType == null)
            {
                TaskDialog.Show("Error", "Wall Type not found: " + wallTypeName);
                tx.RollBack();
                return;
            }

            // Find the Level
            FilteredElementCollector levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level));

            Level selectedLevel = null;
            foreach (Level lvl in levels)
            {
                if (lvl.Name.Equals(levelName, StringComparison.OrdinalIgnoreCase))
                {
                    selectedLevel = lvl;
                    break;
                }
            }
            if (selectedLevel == null)
            {
                TaskDialog.Show("Error", "Level not found: " + levelName);
                tx.RollBack();
                return;
            }

            // Create a Line for the Wall
            Line wallLine = Line.CreateBound(startPoint, endPoint);

            // Create the Wall
            Wall newWall = Wall.Create(doc, wallLine, selectedWallType.Id, selectedLevel.Id, height, 0, false, false);

            tx.Commit();
            TaskDialog.Show("Success", "Wall Created Successfully!");
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



    public static void ChangeWallWidth(Document doc, double newWidth) 
    {

        if (doc == null) return; // Prevent errors if doc is null

        // Collect all wall types in the project
        FilteredElementCollector wallTypeCollector = new FilteredElementCollector(doc)
            .OfClass(typeof(WallType));

        using (Transaction tx = new Transaction(doc, "Change All Wall Widths")) 
        {
            tx.Start();

            foreach (Element elem in wallTypeCollector)
            {
                WallType wallType = elem as WallType;
                if (wallType != null)
                {
                    CompoundStructure structure = wallType.GetCompoundStructure();
                    if (structure != null)
                    {
                        // Adjust the first layer to match the new total width
                        IList<CompoundStructureLayer> layers = structure.GetLayers();
                        if (layers.Count > 0)
                        {
                            // Modify the width of the first layer to the new width
                            layers[0] = new CompoundStructureLayer(newWidth, layers[0].Function, layers[0].MaterialId);

                            // Create a new CompoundStructure with the updated layers
                            CompoundStructure newStructure = structure.Clone();
                            newStructure.SetLayers(layers);

                            // Apply the modified compound structure to the wall type
                            wallType.SetCompoundStructure(newStructure);
                        }
                    }
                }
            }

            tx.Commit();
        }
    }



    public static void DeleteWall(Document doc, )
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
        if (doc == null || wallId == null) return; // Prevent errors if doc or wallId is null

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

            // Set the new eye and target positions for the camera
            view3D.SetCamera(eyePosition, targetPosition);

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
        XYZ currentCameraPosition = orientation.ViewPosition;
        XYZ currentViewDirection = orientation.ViewDirection;

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



}