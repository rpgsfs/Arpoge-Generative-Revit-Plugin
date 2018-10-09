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
using System.Collections;
// using Excel = Microsoft.Office.Interop.Excel;
namespace RevitAddin2
{

  class Room
  {
         private List<Wall> Walls = new List<Wall>();
         private List<ElementId> WallId = new List<ElementId >();
        

         private List<Plane> Planes = new List<Plane>();
         private Floor personalFloor;

        public Floor myFloor
        {
            get
            {
                return personalFloor;
            }
            set
            {
                if(value is Floor)
                    personalFloor = value;
            }
        }

        public Room(Document doc, ElementId levelId, Hexporter hex, int sideLength, int OriginXOffset, int OriginYOffset)
        {
            MakeARoom(doc, levelId, hex);
        }
        public Room(List<Wall> walls)
        {
            Walls.AddRange(walls);
            SetWallIdFirst();
        }

        public void SetWallIdFirst() //ONLY USED ONCE
        {
            foreach(Wall w in Walls)
            {
                WallId.Add(w.Id);
            }
        }

 
        public void MakeARoom(Document doc, ElementId levelId, Hexporter hex, int OriginXOffset, int OriginYOffset)
        {
            
                XYZ[] pointArray = new XYZ[6];
                Curve[] lineArray = new Curve[6];
                for (int i = 0; i < 6; i++)
                {
                    pointArray[i] = new XYZ(hex.GetHexCoordX(i), hex.GetHexCoordY(i),0);
                    System.Diagnostics.Debug.WriteLine(pointArray[i].ToString());
                
                }
            for (int i = 0; i < 6; i++)
            { if (i != 5)
                {
                    lineArray[i] = Line.CreateBound(pointArray[i], pointArray[i + 1]);
                }
                if (i == 5)
                { 
                lineArray[i] = Line.CreateBound(pointArray[i], pointArray[0]);
                }

                }
                for(int i = 0; i < 6; i++)
                 {
                MakeWall(doc, levelId, lineArray[i]); //transactions can only be done 1 at a time
                 }
         
        }



        public void MakeARoom(Document doc, ElementId levelId, Hexporter hex)
        {
            MakeARoom(doc, levelId, hex, 0, 0);
        }

        public List <Wall> GetARoom()
        {
            return Walls;
        }
        public List<ElementId> GetWallId()
        {
            return WallId;
        }
        public  Plane GetMidPlane(Document doc) //This method ALSO WILL selects walls to turn into elements for mirroring
        {
            UIDocument uidoc = new UIDocument(doc); 
            Selection chooseWall = uidoc.Selection; //makes UI to tell user to select an element
            Reference hasPickOne = chooseWall.PickObject(ObjectType.Element);
            Element selectedElement = uidoc.Document.GetElement(hasPickOne);
            if (WallId.Contains(selectedElement.Id))
            { 
                System.Diagnostics.Debug.WriteLine("Hooray!");
                LocationCurve centerLine = selectedElement.Location as LocationCurve;
                XYZ EndP1 = centerLine.Curve.GetEndPoint(0);
                XYZ EndP2 = centerLine.Curve.GetEndPoint(1);
               // double h = (selectedElement as Wall).get_Parameter(BuiltInParameter.WALL_ATTR_HEIGHT_PARAM).AsDouble(); //
                XYZ EndP1above = new XYZ(EndP1.X, EndP1.Y, EndP1.Z + 1);
                return Plane.CreateByThreePoints(EndP1, EndP2, EndP1above);
           }
           else
            {
                System.Diagnostics.Debug.WriteLine("NOT GOOD");
                return GetMidPlane(doc);
            }
            

        }
        public void MakeWall(Document doc, ElementId levelId, Curve wallGuide)
        {
            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start("y");
                Wall temp =  Wall.Create(doc,wallGuide, levelId, false);               
                temp.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(3); //sets the wallGuide line to be the interior face of the hexagon. Default is as center               
                transaction.Commit();
                if(temp != null)
                {
                    Walls.Add(temp);
                    WallId.Add(temp.Id);
                    System.Diagnostics.Debug.WriteLine("Sucsess");
                }
            
            }
        }
        private Plane ExtractPlane(Element e)
        {
            LocationCurve centerLine = e.Location as LocationCurve;
            XYZ EndP1 = centerLine.Curve.GetEndPoint(0);
            XYZ EndP2 = centerLine.Curve.GetEndPoint(1);
            // double h = (selectedElement as Wall).get_Parameter(BuiltInParameter.WALL_ATTR_HEIGHT_PARAM).AsDouble(); //
            XYZ EndP1Above = new XYZ(EndP1.X, EndP1.Y, EndP1.Z + 1);
            return Plane.CreateByThreePoints(EndP1, EndP2, EndP1Above);
        }
        public CurveArray getProfile()
        {
            CurveArray closedProfile = new CurveArray();
            foreach(Wall w in Walls)
            {
                LocationCurve wallLine = w.Location as LocationCurve;
                closedProfile.Append(wallLine.Curve);
            }
            
            return  sortCurveArray(closedProfile,false);
        }

        private CurveArray sortCurveArray(CurveArray unsorted,Boolean Debug)
        {
             unsorted.get_Item(0).GetEndPoint(0);
             CurveArray sorted = new CurveArray();
             sorted.Append(unsorted.get_Item(0));

             while (sorted.Size < 6)
             {
                 for (int i = 0; i < unsorted.Size; i++)
                 {
                    if (sorted.get_Item(sorted.Size - 1).GetEndPoint(1).IsAlmostEqualTo(unsorted.get_Item(i).GetEndPoint(0)))// checks if end of last point = start of the next
                    {
                         sorted.Append(unsorted.get_Item(i));
                         
                     }
                 }
             }
            if (Debug)
            {
                for (int i = 0; i < sorted.Size; i++)
                {
                    System.Diagnostics.Debug.WriteLine("Array Position" + i + "Start Point" + sorted.get_Item(i).GetEndPoint(0).ToString() + "end point" + sorted.get_Item(i).GetEndPoint(1).ToString());
                }
            }
             return sorted;
         //   return unsorted;
        }

    }

}
