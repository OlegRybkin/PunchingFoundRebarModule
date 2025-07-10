﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PunchingFoundRebarModule.Model
{
    internal class ColumnFromLinkFilter : ISelectionFilter
    {
        private readonly Document _hostDoc;

        public ColumnFromLinkFilter(Document hostDoc)
        {
            _hostDoc = hostDoc;
        }

        public bool AllowElement(Element elem)
        {
            return elem is RevitLinkInstance;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            RevitLinkInstance link = _hostDoc.GetElement(reference.ElementId) as RevitLinkInstance;
            Document linkedDoc = link.GetLinkDocument();
            Element linkedElem = linkedDoc.GetElement(reference.LinkedElementId);

            if (linkedElem.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString().ToLower().Contains("пилон") &&
                linkedElem.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM_MT).AsValueString() == "Несущие колонны")
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
