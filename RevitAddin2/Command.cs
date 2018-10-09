#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using RevitAddin2;
using System.Collections;
#endregion

namespace RevitAddin2
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        { 
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            List<Room> Rooms = new List<Room>();

            Level level //finds active level
            = new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .First<Element>(e
              => e.Name.Equals("Level 1"))
                as Level;
            Hexporter h = new Hexporter();
            h.RequestArea();
            Room r = new Room(doc, level.Id, h, 8, 0, 0);
            WallManager wallManager = new WallManager(doc);
            for(int k = 0; k < r.GetWallId().Count; k++)
            {
                Debug.Write("First Room" + "Wall No: " + k + "  " + r.GetWallId()[k]);
            }
            Rooms.Add(r);
            wallManager.takenWalls.AddRange(r.GetARoom());

            for (int i = 0; i < Convert.ToInt32(h.GetIdealArea() / h.GetUnitArea()) - 1; i++)//loops wall adding for amount of walls needed to satisfy area
            {
                FilteredElementCollector coll = new FilteredElementCollector(doc);
                coll.OfClass(typeof(Wall));
                //TODO: add message that tells user to select room to build off
                Selection chooseWall = uidoc.Selection; //makes UI to tell user to select an element
                Reference hasPickOne = chooseWall.PickObject(ObjectType.Element); //adds the wall to the selected elements
                Element selectedElement = uidoc.Document.GetElement(hasPickOne); //gets referenced element                       //TODO:Limit walls selectable
                wallManager.AddToDoors(selectedElement);
                wallManager.BuildARoom(Rooms, selectedElement);
                for(int j = 0; j < Rooms.Count; j++)
                {
                    Debug.WriteLine("Room no." + j);
                    for(int k = 0; k < Rooms[j].GetWallId().Count; k++)
                    {
                        Debug.WriteLine("Element Id No. " + k + Rooms[j].GetWallId()[k].ToString());
                    }
                }
              
            }
          
            wallManager.AddDoors(level);
            wallManager.TrimWalls(9);
           AdderOfElements adderOfElements = new AdderOfElements(doc, Rooms);
           adderOfElements.addFloors(level);
           adderOfElements.addRoofs();
           adderOfElements.addWindows(level);
       //    adderOfElements.addFurnishings(wallManager, level);
           wallManager.KillIntersectors();

            return Result.Succeeded;
        }
    }
}
