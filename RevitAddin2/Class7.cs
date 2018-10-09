using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Structure;

namespace RevitAddin2
{
    class AdderOfElements //this class can add elements afterwards
    {
        private List<Room> useableRooms = new List<Room>();
        private List<Floor> Flooring = new List<Floor>();
        private List<Element> Windowing = new List<Element>();
        Document activeDocument = null;
        FamilySymbol windowSymbol = null;
        FamilySymbol trapDeskSymbol = null;
        public AdderOfElements(Document doc, List<Room> RoomsMade)//TODO: add window selection ui
        {
            activeDocument = doc;
            useableRooms = RoomsMade;
             using (Transaction tx = new Transaction(activeDocument))
            { //loads families into revit project
                tx.Start("Test");
                activeDocument.LoadFamilySymbol("C:\\Users\\Nicholas\\Documents\\Arpoge 2k18\\ScriptTesting\\Window.rfa", "Block Frame_Size as Specified", out windowSymbol);
                activeDocument.LoadFamilySymbol("C:\\Users\\Nicholas\\Documents\\Arpoge 2k18\\ScriptTesting\\TrapTable.rfa", "54X24X29", out trapDeskSymbol);
                if (!windowSymbol.IsActive)
                {
                    windowSymbol.Activate(); doc.Regenerate();                  
                }
                if (!trapDeskSymbol.IsActive)
                {
                    trapDeskSymbol.Activate(); doc.Regenerate();
                }
                tx.Commit();
            }
        }

        public void addFloors(Level level)
        {
            FloorType floorType
  = new FilteredElementCollector(activeDocument)
    .OfClass(typeof(FloorType))
    .First<Element>(
      e => e.Name.Equals("OSB Floor")) //change to floor type we want
      as FloorType;

            foreach (Room RoomToFloor in useableRooms)
            {
                try
                {
                    using (Transaction transaction = new Transaction(activeDocument))
                    {
                        transaction.Start("Flooring");
                        Floor newFloor = activeDocument.Create.NewFloor(RoomToFloor.getProfile(), floorType, level, false, XYZ.BasisZ);
                        transaction.Commit();
                    }
                }
                catch (Exception ex) { }
            }
        }
        public void addRoofs()
        {
            RoofType roofType
  = new FilteredElementCollector(activeDocument)
    .OfClass(typeof(RoofType))
    .First<Element>(
      e => e.Name.Equals("Roof 1")) //change to roof type we want in end (TODO)
      as RoofType;

            double roofElevation = useableRooms[0].GetARoom()[0].get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble(); //gets elevation from wall height
            using (Transaction t = new Transaction(activeDocument))
            {
                t.Start("Roof adding");
                Level level = Level.Create(activeDocument, roofElevation);
                foreach (Room toRoof in useableRooms)
                {
                    ModelCurveArray footPrintToModelCurveMapping = new ModelCurveArray();
                    FootPrintRoof footprintRoof = activeDocument.Create.NewFootPrintRoof(toRoof.getProfile(), level, roofType, out footPrintToModelCurveMapping);
                }
                t.Commit();
            }
        }

        public void addWindows(Level level)
        {
            WallManager wallManager = new WallManager(activeDocument);
            List<Element> Windows = new List<Element>();
       

            using (Transaction t = new Transaction(activeDocument))
            {
                t.Start("adding windows");
                foreach (Wall w in GetOutsideWalls())
                {
                    if (!wallManager.GetDoorWalls().Contains(w))
                    {
                        Windows.Add(activeDocument.Create.NewFamilyInstance(wallManager.GetWallMidPoint(w), windowSymbol, w, level, StructuralType.NonStructural));
                        double headHeight = Windows[Windows.Count - 1].get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM).AsDouble();
                        double sillHeight = Windows[Windows.Count - 1].get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).AsDouble();
                        double wallHeight = useableRooms[0].GetARoom()[0].get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                        Windows[Windows.Count-1].get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set((wallHeight  - (headHeight - sillHeight) ));
                    }
                            
                }
                t.Commit();
                Windowing = Windows;
            }   
        }

        public void addFurnishings(WallManager wallManager, Level level)
        {
            using (Transaction transaction = new Transaction(activeDocument)) {
                transaction.Start("adding a furnishing");
                FamilyInstance desk = activeDocument.Create.NewFamilyInstance(wallManager.GetWallLine(useableRooms[0].GetARoom()[0]), trapDeskSymbol, level, StructuralType.NonStructural);
                Curve c = wallManager.GetWallLine(useableRooms[0].GetARoom()[0]);                
                //ElementTransformUtils.RotateElement(activeDocument, desk.Id, Line.CreateBound(c.GetEndPoint(0), c.GetEndPoint(1)), 60);
                transaction.Commit();
            }

        }
        public List<Wall> GetOutsideWalls()
        {

            BuildingEnvelopeAnalyzerOptions options = new BuildingEnvelopeAnalyzerOptions();
            BuildingEnvelopeAnalyzer analyzer = BuildingEnvelopeAnalyzer.Create(activeDocument, options);
            List<LinkElementId> linkIds = analyzer.GetBoundingElements().ToList();
            List<Wall> exteriorWalls = new List<Wall>();
            if (null != analyzer)
            {

                if (linkIds.Count > 0)
                {
                    foreach (LinkElementId linkId in linkIds)
                    {
                        if (linkId.HostElementId != ElementId.InvalidElementId)
                        {
                            Wall hostWall = activeDocument.GetElement(linkId.HostElementId) as Wall;
                            if (null != hostWall)
                            {
                                exteriorWalls.Add(hostWall);
                            }
                        }
                        else if (linkId.LinkedElementId != ElementId.InvalidElementId)
                        {
                            RevitLinkInstance rvtInstance = activeDocument.GetElement(linkId.LinkInstanceId) as RevitLinkInstance;
                            if (null != rvtInstance)
                            {
                                Wall linkWall = rvtInstance.Document.GetElement(linkId.LinkedElementId) as Wall;
                                if (null != linkWall)
                                {
                                    exteriorWalls.Add(linkWall);
                                }
                            }
                        }
                    }
                }
            }
            return exteriorWalls;
        }

    }
}
