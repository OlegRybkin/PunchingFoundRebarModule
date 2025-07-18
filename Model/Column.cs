﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PunchingFoundRebarModule.Model
{
    internal class Column
    {
        internal XYZ Location { get; private set; }
        internal XYZ FacingOrientation { get; private set; }
        internal double Length { get; private set; }
        internal double Width { get; private set; }

        internal Column(Element element) 
        {
            Element elementType = element.Document.GetElement(element.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsElementId());

            Location = GetLocation(element);
            FacingOrientation = ((FamilyInstance)element).FacingOrientation;
            Length = elementType.LookupParameter("ФОП_РАЗМ_Длина").AsDouble();
            Width = elementType.LookupParameter("ФОП_РАЗМ_Ширина").AsDouble();
        }

        private XYZ GetLocation(Element column)
        {
            //Положение пилона в плане
            XYZ locationPoint = ((LocationPoint)column.Location).Point;
            
            Level downLevel = column.Document.GetElement(column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsElementId()) as Level;

            double downLevelMark = downLevel.ProjectElevation;
            double downOffset = column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).AsDouble();

            XYZ columnLocation = new XYZ
                (
                    locationPoint.X, 
                    locationPoint.Y, 
                    downLevelMark + downOffset);

            return columnLocation;
        }
    }
}
