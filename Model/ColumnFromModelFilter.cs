﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace PunchingFoundRebarModule.Model
{
    internal class ColumnFromModelFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString().ToLower().Contains("пилон") &&
                elem.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM_MT).AsValueString() == "Несущие колонны")
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
