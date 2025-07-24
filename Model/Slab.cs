using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace PunchingFoundRebarModule.Model
{
    internal enum SlabType
    {
        Foundation,
        Plate
    }

    internal class Slab
    {
        internal Element Element { get; private set; }
        internal double RebarCoverUp {  get; set; }
        internal double RebarCoverDown {  get; set; }
        internal double Thickness { get; set; }
        internal SlabType SlabType { get; private set; }

        internal Slab(Element element) 
        {
            Element = element;

            Thickness = element.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();

            RebarCoverUp = GetRebarCoverUpFromModel(element);
            RebarCoverDown = GetRebarCoverDownFromModel(element);

            switch (element.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM_MT).AsValueString())
            {
                case "Фундамент несущей конструкции":
                    SlabType = SlabType.Foundation;
                    break;

                case "Перекрытия":
                    SlabType= SlabType.Plate; 
                    break;
            }
        }

        private double GetRebarCoverUpFromModel(Element element)
        {
            Document doc = element.Document;

            RebarCoverType rebarCoverUpType = doc.GetElement(element.get_Parameter(BuiltInParameter.CLEAR_COVER_TOP).AsElementId()) as RebarCoverType;
            double rebarCoverUp = rebarCoverUpType.CoverDistance;

            return rebarCoverUp;
        }

        private double GetRebarCoverDownFromModel(Element element)
        {
            Document doc = element.Document;

            RebarCoverType rebarCoverDownType = doc.GetElement(element.get_Parameter(BuiltInParameter.CLEAR_COVER_BOTTOM).AsElementId()) as RebarCoverType;
            double rebarCoverDown = rebarCoverDownType.CoverDistance;

            return rebarCoverDown;
        }
    }
}
