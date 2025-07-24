﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace PunchingFoundRebarModule.Model
{
    internal class SlabFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM_MT).AsValueString() == "Фундамент несущей конструкции" ||
                elem.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM_MT).AsValueString() == "Перекрытия")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
