using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace PunchingFoundRebarModule.Model
{
    internal class PunchingFoundRebarTools
    {
        static internal void AddPunchingRebarToFoundation
            (Document doc, 
            Element foundationSlab, 
            Column column,
            double step,
            double rebarDiameter,
            double backRebarDiameter,
            double rebarCoverUp,
            double rebarCoverDown)
        {
            string familyName = "IFC_Каркас_КГор_1";
            string familyType = "Х_1501";

            double longRebarDiameter = 10 / 304.8; // диаметр продольной арматуры каркаса

            FilteredElementCollector FEC = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol));
            FamilySymbol punchingRebarFS = FEC.ToElements().Cast<FamilySymbol>()
                                                .Where(x => x.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString() == familyName)
                                                .Where(x => x.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString() == familyType)
                                                .FirstOrDefault();

            if (punchingRebarFS == null)
            {
                TaskDialog.Show("Ошибка", $"Семейство \"{familyName}\" не найдено в модели");
            }
            else
            {
                if (!punchingRebarFS.IsActive)
                {
                    punchingRebarFS.Activate();
                    doc.Regenerate();
                }

                double punchingRebarHeight = GetFoundationSlabHeight(foundationSlab) - (rebarCoverUp + backRebarDiameter) - (rebarCoverDown + 2 * backRebarDiameter) - longRebarDiameter;
                int stirrupCount = Convert.ToInt32(GetFrameLength(foundationSlab, step, rebarCoverDown, backRebarDiameter) / step) + 1;

                // Создние каркаса в первом направлении (торцы каркасов идут вдоль длиннной стороны пилона)
                PunchingRebar punchingRebar0 = new PunchingRebar(punchingRebarFS, column.Location, step, rebarDiameter, longRebarDiameter, punchingRebarHeight, stirrupCount);

                MoveFamilyInstanceDown(punchingRebar0, longRebarDiameter, rebarDiameter, rebarCoverUp, backRebarDiameter);
                RotateFamilyInstance(punchingRebar0, column.FacingOrientation.AngleTo(punchingRebar0.FamilyInstance.FacingOrientation));
                MoveFamilyInstanceCrossColumn(punchingRebar0, foundationSlab, column, rebarCoverDown, backRebarDiameter, false);

                int punchingRebarCount0 = GetPunchingRebarCount(column, step, foundationSlab, rebarCoverDown, backRebarDiameter, false);

                MoveFamilyInstanceAlongColumn0(punchingRebar0, column, step, foundationSlab, rebarCoverDown, backRebarDiameter);
                CopyRebar(punchingRebar0.FamilyInstance, -column.FacingOrientation, punchingRebarCount0, step * 2);

                Element element = MirrorRebar(punchingRebar0, column);
                CopyRebar((FamilyInstance)element, -column.FacingOrientation, punchingRebarCount0, step * 2);


                // Создание каркаса во втором направлении (торцы каркасов идут вдоль короткой стороны пилона)
                PunchingRebar punchingRebar90 = new PunchingRebar(punchingRebarFS, column.Location, step, rebarDiameter, longRebarDiameter, punchingRebarHeight, stirrupCount);

                MoveFamilyInstanceDown(punchingRebar90, longRebarDiameter, rebarDiameter, rebarCoverUp, backRebarDiameter);
                RotateFamilyInstance(punchingRebar90, column.FacingOrientation.AngleTo(punchingRebar90.FamilyInstance.FacingOrientation) + Math.PI / 2);
                MoveFamilyInstanceCrossColumn(punchingRebar90, foundationSlab, column, rebarCoverDown, backRebarDiameter, true);

                int punchingRebarCount90 = GetPunchingRebarCount(column, step, foundationSlab, rebarCoverDown, backRebarDiameter, true);

                //MessageBox.Show(GetPunchingRebarCount0(column, step, foundationSlab, rebarCoverDown, backRebarDiameter).ToString());
            }
        }
        
        static private Element MirrorRebar(PunchingRebar punchingRebar, Column column)
        {
            Plane plane = Plane.CreateByThreePoints
                (
                    column.Location,
                    new XYZ
                        (
                            column.Location.X + 10 * column.FacingOrientation.X,
                            column.Location.Y + 10 * column.FacingOrientation.Y,
                            column.Location.Z
                        ),
                    new XYZ
                        (
                            column.Location.X,
                            column.Location.Y,
                            column.Location.Z + 10
                        )
                );

            IList<ElementId> mirrorElements = ElementTransformUtils.MirrorElements(punchingRebar.FamilyInstance.Document, new List<ElementId>() {punchingRebar.FamilyInstance.Id}, plane, true);

            Element element = punchingRebar.FamilyInstance.Document.GetElement(mirrorElements.FirstOrDefault());  

            return element;
        }


        static private void CopyRebar(FamilyInstance familyInstance, XYZ direction, int count, double step)
        {
            XYZ startPoint = ((LocationPoint)familyInstance.Location).Point;

            for (int i = 1; i < count; i++)
            {
                XYZ endPoint = new XYZ
                    (
                        startPoint.X + direction.X * step * i,
                        startPoint.Y + direction.Y * step * i,
                        startPoint.Z
                    );

                ElementTransformUtils.CopyElement(familyInstance.Document, familyInstance.Id, endPoint - startPoint);

                startPoint = endPoint;
            }
        }

        /// <summary>
        /// Возращает количество каркасов, которые расположены перпендикулярно длинной стороне
        /// </summary>
        /// <param name="column"></param>
        /// <param name="step"></param>
        /// <param name="foundationSlab"></param>
        /// <param name="rebarCoverDown"></param>
        /// <param name="backRebarDiameter"></param>
        /// <returns></returns>
        static private int GetPunchingRebarCount(Column column, double step, Element foundationSlab, double rebarCoverDown, double backRebarDiameter, bool isAngle90)
        {
            double punchingLength = 0;
            int punchingRebarCount = 0;

            if (isAngle90)
            {
                double afterColumnDistance = GetAfterColumnDistance(foundationSlab, rebarCoverDown, backRebarDiameter);
                punchingLength = column.Width + 2 * afterColumnDistance;
                punchingRebarCount = Convert.ToInt32(Math.Floor(punchingLength / (2 * step)));
            }
            else
            {
                double punchingZone = GetPunchingZone(foundationSlab, rebarCoverDown, backRebarDiameter);
                punchingLength = column.Length + 2 * punchingZone;
                punchingRebarCount = Convert.ToInt32(Math.Ceiling(punchingLength / (2 * step))) + 1;
            }

            return punchingRebarCount;
        }

        /// <summary>
        /// Перемещает каркас вдоль длинной стороны пилона в исходную точку для копирования
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="column"></param>
        /// <param name="step"></param>
        /// <param name="foundationSlab"></param>
        /// <param name="rebarCoverDown"></param>
        /// <param name="backRebarDiameter"></param>
        static private void MoveFamilyInstanceAlongColumn0(PunchingRebar punchingRebar, Column column, double step, Element foundationSlab, double rebarCoverDown, double backRebarDiameter)
        {
            int punchingRebarCount = GetPunchingRebarCount0(column, step, foundationSlab, rebarCoverDown, backRebarDiameter);
            
            XYZ location = ((LocationPoint)punchingRebar.FamilyInstance.Location).Point;

            XYZ startPoint = new XYZ
                (
                    location.X + punchingRebar.FamilyInstance.FacingOrientation.X * 0.5 * punchingRebar.FamilyInstance.LookupParameter("мод_ПР_Шаг по ширине").AsDouble(),
                    location.Y + punchingRebar.FamilyInstance.FacingOrientation.Y * 0.5 * punchingRebar.FamilyInstance.LookupParameter("мод_ПР_Шаг по ширине").AsDouble(),
                    location.Z
                );

            XYZ endPoint = new XYZ
                (
                    location.X + column.FacingOrientation.X * 0.5 * (punchingRebarCount * (2 * step) - 2 * step),
                    location.Y + column.FacingOrientation.Y * 0.5 * (punchingRebarCount * (2 * step) - 2 * step),
                    location.Z
                );



            punchingRebar.FamilyInstance.Location.Move(endPoint - startPoint);
        }


        /// <summary>
        /// Перемещает каркас перпендикулярно длинной стороне пилона на необходимое расстояние от грани пилона
        /// </summary>
        /// <param name="foundationSlab"></param>
        /// <param name="familyInstance"></param>
        /// <param name="column"></param>
        /// <param name="rebarCoverDown"></param>
        /// <param name="backRebarDiameter"></param>
        /// <param name="isAngle90">Условный параметр, который обознеачет еаправление каркаса (для каркаса с индексом 90)</param>
        static private void MoveFamilyInstanceCrossColumn(PunchingRebar punchingRebar, Element foundationSlab,  Column column, double rebarCoverDown, double backRebarDiameter, bool isAngle90)
        {
            XYZ startPoint = new XYZ();
            XYZ endPoint = new XYZ();

            if (isAngle90)
            {
                startPoint = punchingRebar.Location;
                endPoint = column.Location;
            }
            else
            {
                double afterColumnDistance = GetAfterColumnDistance(foundationSlab, rebarCoverDown, backRebarDiameter);
                double length = punchingRebar.FamilyInstance.LookupParameter("мод_Х_Шаг").AsDouble() * (punchingRebar.FamilyInstance.LookupParameter("мод_Х_Количество").AsInteger() - 1) +
                    2 * punchingRebar.FamilyInstance.LookupParameter("мод_ПР_Анкеровка_Верх_1").AsDouble();

                startPoint = ((LocationPoint)punchingRebar.FamilyInstance.Location).Point;
                endPoint = new XYZ
                    (
                        startPoint.X - column.FacingOrientation.Y * (length / 2 + column.Width / 2 + afterColumnDistance - punchingRebar.FamilyInstance.LookupParameter("мод_ПР_Анкеровка_Верх_1").AsDouble()),
                        startPoint.Y + column.FacingOrientation.X * (length / 2 + column.Width / 2 + afterColumnDistance - punchingRebar.FamilyInstance.LookupParameter("мод_ПР_Анкеровка_Верх_1").AsDouble()),
                        startPoint.Z
                    );
            }

            punchingRebar.FamilyInstance.Location.Move(endPoint - startPoint);
        }


        /// <summary>
        /// Перемещает семейство из верхней точки вставки вниз на необходимую с учетом фоновой арматуры
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="longRebarDiameter"></param>
        /// <param name="rebarDiameter"></param>
        /// <param name="rebarCoverUp"></param>
        /// <param name="backRebarDiameter"></param>
        static private void MoveFamilyInstanceDown(PunchingRebar punchingRebar, double longRebarDiameter, double rebarDiameter ,double rebarCoverUp, double backRebarDiameter)
        {
            XYZ startPoint = ((LocationPoint)punchingRebar.FamilyInstance.Location).Point;
            XYZ endPoint = new XYZ
                (
                    startPoint.X,
                    startPoint.Y,
                    startPoint.Z + (rebarDiameter + 0.5 * 5 * rebarDiameter) - 0.5 * longRebarDiameter - rebarCoverUp - backRebarDiameter
                );

            punchingRebar.FamilyInstance.Location.Move(endPoint - startPoint); 
        }


        /// <summary>
        /// Поворачивает семейство на заданный угол вокруг своей оси
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="angle"></param>
        static private void RotateFamilyInstance(PunchingRebar punchingRebar, double angle)
        {
            LocationPoint locationPoint = punchingRebar.FamilyInstance.Location as LocationPoint;
            Line axeLine = Line.CreateBound(locationPoint.Point, new XYZ(locationPoint.Point.X, locationPoint.Point.Y, locationPoint.Point.Z + 1));

            punchingRebar.FamilyInstance.Location.Rotate(axeLine, angle);
        }

        /// <summary>
        /// Находит рабочую высоту сечения фундамента (h0)
        /// </summary>
        /// <returns></returns>
        static private double GetWorkingHeight(Element foundationSlab, double rebarCoverDown, double backRebarDiameter)
        {
            double foundationSlabHeight = GetFoundationSlabHeight(foundationSlab);
            double workingHeight = foundationSlabHeight - (rebarCoverDown + backRebarDiameter);

            return workingHeight;
        }

        /// <summary>
        /// Находит расстояние от грани колонны до первого стержня каркаса
        /// </summary>
        /// <returns></returns>
        static private double GetAfterColumnDistance(Element foundationSlab, double rebarCoverDown, double backRebarDiameter)
        {
            double workingHeight = GetWorkingHeight(foundationSlab, rebarCoverDown, backRebarDiameter);
            double afterColumnDistance = workingHeight / 3;
            double afterColumnDistanceRounded = Math.Ceiling((afterColumnDistance * 304.8) / 10) * 10 / 304.8;

            return afterColumnDistanceRounded;
        }

        /// <summary>
        /// Находит размер зоны продавливания (расстояние от грани пилона до 1,5h0)
        /// </summary>
        /// <param name="foundationSlab"></param>
        /// <param name="rebarCoverDown"></param>
        /// <param name="backRebarDiameter"></param>
        /// <returns></returns>
        static private double GetPunchingZone(Element foundationSlab, double rebarCoverDown, double backRebarDiameter)
        {
            double workingHeight = GetWorkingHeight(foundationSlab, rebarCoverDown, backRebarDiameter);
            double punchingZone = 1.5 * workingHeight;

            return punchingZone;
        }

        /// <summary>
        /// Находит длину каркаса (без учета "хвостиков")
        /// </summary>
        /// <param name="foundationSlab"></param>
        /// <param name="step"></param>
        /// <param name="rebarCoverDown"></param>
        /// <param name="backRebarDiameter"></param>
        /// <returns></returns>
        static private double GetFrameLength(Element foundationSlab, double step, double rebarCoverDown, double backRebarDiameter)
        {
            double workingHeight = GetWorkingHeight(foundationSlab, rebarCoverDown, backRebarDiameter);
            double afterColumnDistance = GetAfterColumnDistance(foundationSlab, rebarCoverDown, backRebarDiameter);
            double punchingZoneLength = GetPunchingZone(foundationSlab, rebarCoverDown, backRebarDiameter);
            double punchingZoneLengthRounded = Math.Ceiling(punchingZoneLength * 304.8 /10) * 10 / 304.8;
            double frameLength = Math.Ceiling((punchingZoneLengthRounded - afterColumnDistance) / step) * step;
            
            return frameLength;
        }


        /// <summary>
        /// Определяет толщину фундаментной плиты
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        static private double GetFoundationSlabHeight (Element element)
        {
            double foundationSlabHeight = element.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();

            return foundationSlabHeight;
        }

        



    }
}
