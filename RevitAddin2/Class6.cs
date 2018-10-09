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
using Autodesk.Revit.DB.Structure;

namespace RevitAddin2
{
    class ElementAdder
    {
        public ElementAdder(Document doc)
        {

        }

        public void AddFloor(Document doc, Room extractingProfile,Level level)
        {
            doc.Create.NewFloor(extractingProfile.getProfile(), null, level, false, null);
        }
    }

}
