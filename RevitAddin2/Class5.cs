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

namespace RevitAddin2
{
    class ParameterSetter //as more parameter sets are needed, add more of these methods. Also, this is the RIGHT way to do getting and setting
    {
        Element el;
        public ParameterSetter(Element e)
        {
            el = e;
        }

        Parameter _p(BuiltInParameter bip)
        {
            return el.get_Parameter(bip);
        }

        public int ChangeHeight
        {
            get { return _p(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsInteger(); }
            set { _p(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(value); }
        }
    }
}
