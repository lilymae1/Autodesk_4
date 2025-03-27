using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

public class RevitCommandHandler : IExternalEventHandler
{
    private string _command;
    private Dictionary<string, object> _parameters;
    private UIApplication _uiApp;

    public RevitCommandHandler(UIApplication uiApp)
    {
        _uiApp = uiApp;
    }

    public void SetCommand(string command, Dictionary<string, object> parameters)
    {
        _command = command;
        _parameters = parameters;
    }

    public void Execute(UIApplication uiApp)
    {
        try
        {
            Document doc = uiApp.ActiveUIDocument?.Document;
            if (doc == null)
            {
                Logger.Log("No active document found.");
                return;
            }

            if (doc.IsReadOnly)
            {
                Logger.Log("Error: The document is currently read-only and cannot be modified.");
                return;
            }

            Logger.Log("Transaction started successfully.");

            switch (_command)
            {
                case "CreateWall":
                    XYZ start = new XYZ(Convert.ToDouble(_parameters["startX"]), Convert.ToDouble(_parameters["startY"]), 0);
                    XYZ end = new XYZ(Convert.ToDouble(_parameters["endX"]), Convert.ToDouble(_parameters["endY"]), 0);
                    double height = Convert.ToDouble(_parameters["height"]);
                    string wallType = _parameters["wallType"].ToString();
                    string level = _parameters["level"].ToString();

                    AICommands.CreateWall(doc, start, end, height, wallType, level);
                    break;

                case "ModifyWallHeight":
                    AICommands.ModifyWallHeights(doc, Convert.ToDouble(_parameters["newHeight"]));
                    break;

                case "DeleteWall":
                    AICommands.DeleteWall(doc, new ElementId(Convert.ToInt32(_parameters["wallId"])));
                    break;

                    case "DeleteAllWalls":
                    AICommands.DeleteAllWalls(doc);
                    break;

                case "MoveCamera":
                    XYZ eyePosition = new XYZ(
                    Convert.ToDouble(_parameters["eyeX"]),
                    Convert.ToDouble(_parameters["eyeY"]),
                    Convert.ToDouble(_parameters["eyeZ"])
                    );
                    XYZ targetPosition = new XYZ(
                    Convert.ToDouble(_parameters["targetX"]),
                    Convert.ToDouble(_parameters["targetY"]),
                    Convert.ToDouble(_parameters["targetZ"])
                    );
                    AICommands.MoveCamera(doc, view3D, eyePosition, targetPosition);
                    break;

                case "ChangeViewType":
                    AICommands.ChangeViewType(doc, view, _parameters["newViewType"].ToString());
                    break;

                case "ViewRotation":
                    AICommands.RotateCamera(Convert.ToDouble(_parameters["angle"], uiApp));
                    break;
                
                    // **Create Roof Case**
                case "CreateRoof":
                    ElementId roofLevelId = new ElementId(Convert.ToInt32(_parameters["levelId"])); // Assuming it's passed as an ID
                    ElementId roofTypeId = new ElementId(Convert.ToInt32(_parameters["roofTypeId"])); // Assuming it's passed as an ID
                    CurveArray roofBoundary = new CurveArray(); // You would need to convert _parameters into curves
                    // Example of creating a simple rectangular boundary (to be replaced with real data)
                    roofBoundary.Append(Line.CreateBound(new XYZ(0, 0, 0), new XYZ(10, 0, 0)));
                    roofBoundary.Append(Line.CreateBound(new XYZ(10, 0, 0), new XYZ(10, 10, 0)));
                    roofBoundary.Append(Line.CreateBound(new XYZ(10, 10, 0), new XYZ(0, 10, 0)));
                    roofBoundary.Append(Line.CreateBound(new XYZ(0, 10, 0), new XYZ(0, 0, 0)));

                    AICommands.CreateRoof(doc, roofLevelId, roofTypeId, roofBoundary);
                    break;

                    // **Create Window Case**
                case "CreateWindow":
                    ElementId windowWallId = new ElementId(Convert.ToInt32(_parameters["wallId"])); // Assuming wall ID is passed
                    ElementId windowTypeId = new ElementId(Convert.ToInt32(_parameters["windowTypeId"])); // Assuming window type ID is passed
                    XYZ windowLocation = new XYZ(
                    Convert.ToDouble(_parameters["windowX"]),
                    Convert.ToDouble(_parameters["windowY"]),
                    Convert.ToDouble(_parameters["windowZ"])
                    );

                    AICommands.CreateWindow(doc, windowWallId, windowTypeId, windowLocation);
                    break;

                // **Create Door Case**
                case "CreateDoor":
                    ElementId doorWallId = new ElementId(Convert.ToInt32(_parameters["wallId"])); // Assuming wall ID is passed
                    ElementId doorTypeId = new ElementId(Convert.ToInt32(_parameters["doorTypeId"])); // Assuming door type ID is passed
                    XYZ doorLocation = new XYZ(
                    Convert.ToDouble(_parameters["doorX"]),
                    Convert.ToDouble(_parameters["doorY"]),
                    Convert.ToDouble(_parameters["doorZ"])
                    );

                    AICommands.CreateDoor(doc, doorWallId, doorTypeId, doorLocation);
                    break;
                default:
                    Logger.Log("Unknown command received.");
                    break;
            }
            Logger.Log("Transaction committed successfully.");
        }
        catch (Exception ex)
        {
            Logger.Log("Error in command execution: " + ex.Message);
        }
    }

    public string GetName()
    {
        return "RevitCommandHandler";
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
