using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System.Diagnostics;

namespace RevitAddin2
{
    class WallManager // a class wholly devoted to managing walls
    {
        static private Document doc;
        private List<Wall> allTheWalls = new List<Wall>();
        public List<Wall> takenWalls = new List<Wall>();//im sorry
        private List<Wall> freeWalls = new List<Wall>();
        private List<Element> doorWalls = new List<Element>();

        private FamilySymbol doorSymbol = null;

        public WallManager(Document d)
        {
            doc = d;
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Adding Door symbol");
                //      doc.LoadFamilySymbol("C:\\Users\\Nicholas DiLauro\\Documents\\Aproge Inventor\\Revit Scripting\\Sliding Door.rfa", "1800x2200", out doorSymbol); //file type must be changed per user, find better way
                doc.LoadFamilySymbol("C:\\Users\\Nicholas\\Documents\\Arpoge 2k18\\ScriptTesting\\SlidingDoor.rfa","1800x2200", out doorSymbol);
                if (!doorSymbol.IsActive)
                { doorSymbol.Activate(); doc.Regenerate(); }
                tx.Commit();
            }
        }
        public Face getWallExterior(Wall pickedWall)
        {           
            // Get the side faces
            IList<Reference> sideFaces =
                HostObjectUtils.GetSideFaces(pickedWall,
                  ShellLayerType.Exterior);
            // access the side face
            Face face =
                doc.GetElement(sideFaces[0])
                .GetGeometryObjectFromReference(sideFaces[0]) as Face;
            return face;
        }
        public void BuildARoom(List <Room> roomsMade, Element selectedElement)
        {
            List<ElementId> wallsToMirror = new List<ElementId>();//Ids to use in mirroring arguement
            List<Wall> wallsToAdd = new List<Wall>(); //Walls to add to TAKEN 
            List<Wall> wallsToAddToRoom = new List<Wall>();
            Plane mirrorPlane = null;//try to replace with "out" later
            foreach(Room r in roomsMade)
            {
                if (r.GetWallId().Contains(selectedElement.Id))
                {
                    mirrorPlane = ExtractPlane(selectedElement); //gets plane for mirroring

                    foreach(ElementId w in r.GetWallId())
                    {
                        if (!w.Equals(selectedElement.Id))
                        {
                            wallsToMirror.Add(w);
                        }
                    }
                    break;
                }
            }
            using (Transaction transaction = new Transaction(doc))//mirrors walls
            {

                transaction.Start("Mirroring");
                ElementTransformUtils.MirrorElements(doc, wallsToMirror, ExtractPlane(selectedElement), true);//checked: fine
               // FailuresAccessor a = new FailuresAccessor(0);
             //   a.DeleteAllWarnings();
                transaction.Commit();
            }
            foreach(Wall w in getAllWalls())//BAD code, PLEASE learn filters later!
            {
                Boolean wallAllower = true;
                foreach (Wall takenWall in takenWalls)
                {
                    if(w.Id.IntegerValue == takenWall.Id.IntegerValue)
                    {
                        wallAllower = false;
                    }
                   
                }
                if (wallAllower)
                {
                    wallsToAdd.Add(w);
                }

            }
            wallsToAddToRoom.AddRange(wallsToAdd);
            wallsToAddToRoom.Add(selectedElement as Wall);
            Debug.WriteLine("TAKEN WALLS SIZE" + takenWalls.Count);
            roomsMade.Add(new Room(wallsToAddToRoom));
            takenWalls.AddRange(wallsToAdd);
            
        }

        private List<Element> getAllWalls()
        {
            FilteredElementCollector coll = new FilteredElementCollector(doc);
            coll.OfClass(typeof(Wall));
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            IList<Element> allWalls = coll.WherePasses(filter).WhereElementIsNotElementType().ToElements();
            return allWalls.ToList<Element>();
        }
        private Plane ExtractPlane(Element wallToBeMirrored)
        {
            LocationCurve centerLine = wallToBeMirrored.Location as LocationCurve;
            XYZ EndP1 = centerLine.Curve.GetEndPoint(0);
            XYZ EndP2 = centerLine.Curve.GetEndPoint(1);
            // double h = (selectedElement as Wall).get_Parameter(BuiltInParameter.WALL_ATTR_HEIGHT_PARAM).AsDouble(); //
            XYZ EndP1Above = new XYZ(EndP1.X, EndP1.Y, EndP1.Z + 1);
            return Plane.CreateByThreePoints(EndP1, EndP2, EndP1Above);
        }
        public void TrimWalls(int newHeight)
        {
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("trimming");
                foreach (Wall w in takenWalls)
                {

                    ParameterSetter p = new ParameterSetter(w);
                    p.ChangeHeight = newHeight;
                }
                tx.Commit();
            }
        }

        public List<ElementId> GetWallIds() //gets Ids of all taken walls
        {
            List<ElementId> i = new List<ElementId>();
            foreach(Wall w in takenWalls)
            {
                Debug.WriteLine("Taken Wall ID" + w.Id);
                i.Add(w.Id);
            }
            return i;
        }

        public void AddToDoors(Element w)
        {
            doorWalls.Add(w);
        }
        public List<Element> GetDoorWalls()
        {
            return doorWalls;
        }

        public XYZ GetWallMidPoint(Element e)
        {
            LocationCurve centerLine = e.Location as LocationCurve;
            XYZ EndP1 = centerLine.Curve.GetEndPoint(0);
            XYZ EndP2 = centerLine.Curve.GetEndPoint(1);
            XYZ location = (EndP1 + EndP2) / 2;
            return location;
        }
        public Line GetWallLine(Wall w)
        {
            LocationCurve centerLine = w.Location as LocationCurve;
            XYZ EndP1 = centerLine.Curve.GetEndPoint(0);
            XYZ EndP2 = centerLine.Curve.GetEndPoint(1);
            return Line.CreateBound(EndP1, EndP2);
        }
        public void KillIntersectors()
        {
            List < Element > allWalls = getAllWalls();
            List<ElementId> filteredWalls = new List<ElementId>();
            List < LocationPoint > filteringLocation = new List<LocationPoint>();
            List<ElementId> wallsToDelete = new List<ElementId>();
            List<XYZ> pointsToString = new List<XYZ>();
            List<String> stringsContained = new List<String>();

            foreach(Element e in allWalls)
            {
                XYZ tempPoint = GetWallMidPoint(e);
                if(!stringsContained.Contains(tempPoint.ToString())) 
                {
                    Debug.Write(filteringLocation.Count);
                    XYZ point = GetWallMidPoint(e);
                    stringsContained.Add(point.ToString());
                    filteredWalls.Add(e.Id);
                }
            }
            foreach(Element e in allWalls)
            {
                if (!filteredWalls.Contains(e.Id))
                {
                    wallsToDelete.Add(e.Id);
                }
            }
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Deletions");
                doc.Delete(wallsToDelete);
                tx.Commit();
            }

        }


        public void AddDoors(Level l)
        {
            foreach(Wall w in doorWalls)
            {
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Dooring");
                    FamilyInstance door = doc.Create.NewFamilyInstance(GetWallMidPoint(w), doorSymbol, w, l, StructuralType.NonStructural);
                    tx.Commit();

                }
            }
        }

        
    
    }
}
