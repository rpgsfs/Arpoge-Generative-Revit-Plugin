using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
//using Excel = Microsoft.Office.Interop.Excel
//everything is a double, because the XYZ constructor will just take a double anyways
namespace RevitAddin2
{
    class Hexporter
    {
        static private double area;
        static private double sideLength; 
        static private double x0;
        static private double x1;
        static private double x2;
        static private double x3;
        static private double x4;
        static private double x5;
        static private double y0;
        static private double y5;
        static private double y1;
        static private double y4;
        static private double y2;
        static private double y3;


        public void RequestArea()
        {
           try {
                area = Convert.ToDouble(Microsoft.VisualBasic.Interaction.InputBox("Total Area of floorplan?", "Office Area", "Please input your requested area, in square feet"));
                sideLength = Convert.ToDouble(Microsoft.VisualBasic.Interaction.InputBox("Sidelength of Walls", "HexLength", "Please input your requested area, in feet"));
                //    sideLength = Math.Sqrt(((area * 2.0)) / (3 * Math.Sqrt(3))); //aussuming regular hexagon, turns area into sidelength
                ComputeLengths();
               }
            catch(FormatException ex) { RequestArea(); } //for when a number isnt put in


        }
        /* 5-----0
          -      -
        -          -
        4           1  //hex verticy identifier
         -        -
           -      -
           3----2 */
     // 0__
 //  _ 5/  \1__ sides
 // /  4\__/ 2 \
 // \__/ 3 \__/
        public double GetSideLength()
        {
            return sideLength;
        }
        public double GetIdealArea()
        {
            return area;
        }
        public double GetUnitArea()
        {
            return Math.Pow(sideLength, 2) * (3 * Math.Sqrt(3)) / 2;
        }
        public double GetHexCoordX(int selectedVertex)
        {
            double[] xArray = { x0, x1, x2, x3, x4, x5 };
            return xArray[selectedVertex];
        }

        public double GetHexCoordY(int selectedVertex)
        {
            
            double[] yArray = { y0, y1, y2, y3, y4, y5};
            return yArray[selectedVertex];

        }
       
        public void ComputeLengths(int originXOffset, int originYOffset)
        {
            x0 = (sideLength / 2)+ originXOffset;
            x1 = sideLength+ originXOffset;
            x2 = x0;
            x3 = -x2;
            x4 = -x1;
            x5 = -x2;
            y0 = (Math.Sqrt(3) * (sideLength) / 2)+ originYOffset;
            y5 = y0;
            y1 = 0;
            y4 = y1;
            y2 = -y0;
            y3 = y2;
        }
        public void ComputeLengths()
        {
            ComputeLengths(0, 0);
        }


       }


    }

