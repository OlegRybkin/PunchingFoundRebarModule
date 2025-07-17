using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using RevitTools;
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
    internal class PunchingRebarPlacementService
    {
        static internal void AddPunchingRebarToFoundation
            (Document doc, 
            Element foundationSlab, 
            Column column,
            double stirrupStep,
            double frameStep,
            double rebarDiameter,
            double backRebarDiameter,
            double rebarCoverUp,
            double rebarCoverDown)
        {
            string familyName = "IFC_Каркас_КГор_1";
            string familyType = "Х_1501";

            double longRebarDiameter = Calculator.FromMmToFeet(10); // диаметр продольной арматуры каркаса

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

                // ПОД КАРКАСОМ С УГЛОМ 90 ПОНИМАЕТСЯ КАРКАС, ТОРЕЦ КОТОРОГО ЛЕЖИТ ПАРАЛЛЕЛЬНО КОРОТКОЙ СТОРОНЕ ПИЛОНА

                double punchingRebarHeight = PunchingRebarGeometryCalculator.GetFoundationSlabHeight(foundationSlab) - (rebarCoverUp + backRebarDiameter) - (rebarCoverDown + 2 * backRebarDiameter) - longRebarDiameter;
                int stirrupCount = Convert.ToInt32(PunchingRebarGeometryCalculator.GetFrameLength(foundationSlab, stirrupStep, rebarCoverDown, backRebarDiameter) / stirrupStep) + 1;

                // Создние каркаса в первом направлении (торцы каркасов идут вдоль длиннной стороны пилона)
                PunchingRebar punchingRebar0 = new PunchingRebar(punchingRebarFS, column.Location, stirrupStep, rebarDiameter, longRebarDiameter, punchingRebarHeight, stirrupCount);

                MoveFamilyInstanceDown(punchingRebar0, longRebarDiameter, rebarDiameter, rebarCoverUp, backRebarDiameter);
                RotateFamilyInstance(punchingRebar0, column.FacingOrientation.AngleTo(punchingRebar0.FamilyInstance.FacingOrientation));
                MoveFamilyInstanceCrossColumn(punchingRebar0, foundationSlab, column, step, rebarCoverDown, backRebarDiameter, false);

                int punchingRebarCount0 = GetPunchingRebarCount(column, step, foundationSlab, rebarCoverDown, backRebarDiameter, false);

                MoveFamilyInstanceAlongColumn(punchingRebar0, column, step, foundationSlab, rebarCoverDown, backRebarDiameter, false);
                CopyRebar(punchingRebar0.FamilyInstance, -column.FacingOrientation, punchingRebarCount0, step * 2);

                Element element0 = MirrorRebar(punchingRebar0, column, false);
                CopyRebar((FamilyInstance)element0, -column.FacingOrientation, punchingRebarCount0, step * 2);


                // Создание каркаса во втором направлении (торцы каркасов идут вдоль короткой стороны пилона)
                PunchingRebar punchingRebar90 = new PunchingRebar(punchingRebarFS, column.Location, step, rebarDiameter, longRebarDiameter, punchingRebarHeight, stirrupCount);

                MoveFamilyInstanceDown(punchingRebar90, longRebarDiameter, rebarDiameter, rebarCoverUp, backRebarDiameter);
                RotateFamilyInstance(punchingRebar90, column.FacingOrientation.AngleTo(punchingRebar90.FamilyInstance.FacingOrientation) + Math.PI / 2);
                MoveFamilyInstanceCrossColumn(punchingRebar90, foundationSlab, column, step, rebarCoverDown, backRebarDiameter, true);

                int punchingRebarCount90 = GetPunchingRebarCount(column, step, foundationSlab, rebarCoverDown, backRebarDiameter, true);

                MoveFamilyInstanceAlongColumn(punchingRebar90, column, step, foundationSlab, rebarCoverDown, backRebarDiameter, true);

                XYZ copyOrientation = new XYZ
                    (
                        -column.FacingOrientation.Y,
                        column.FacingOrientation.X,
                        column.FacingOrientation.Z

                    );

                CopyRebar(punchingRebar90.FamilyInstance, -copyOrientation, punchingRebarCount90, step * 2);

                Element element90 = MirrorRebar(punchingRebar90, column, true);
                CopyRebar((FamilyInstance)element90, -copyOrientation, punchingRebarCount90, step * 2);
            }
        }
        
        static private Element MirrorRebar(PunchingRebar punchingRebar, Column column, bool isAngle90)
        {
            Plane plane = null;

            if (isAngle90)
            {
                plane = Plane.CreateByThreePoints
                    (
                        column.Location,
                        new XYZ
                        (
                            column.Location.X - 10 * column.FacingOrientation.Y,
                            column.Location.Y + 10 * column.FacingOrientation.X,
                            column.Location.Z
                        ),

                        new XYZ
                        (
                            column.Location.X,
                            column.Location.Y,
                            column.Location.Z + 10
                        )
                    );
            }
            else
            {
                plane = Plane.CreateByThreePoints
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
            }
            

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
                double afterColumnDistance = PunchingRebarGeometryCalculator.GetAfterColumnDistance(foundationSlab, rebarCoverDown, backRebarDiameter);
                punchingLength = column.Width + 2 * afterColumnDistance;
                punchingRebarCount = Convert.ToInt32(Math.Floor(punchingLength / (2 * step)));
            }
            else
            {
                double punchingZone = PunchingRebarGeometryCalculator.GetPunchingZone(foundationSlab, rebarCoverDown, backRebarDiameter);
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
        static private void MoveFamilyInstanceAlongColumn(PunchingRebar punchingRebar, Column column, double step, Element foundationSlab, double rebarCoverDown, double backRebarDiameter, bool isAngle90)
        {
            XYZ startPoint = new XYZ();
            XYZ endPoint = new XYZ();

            if (isAngle90)
            {
                int punchingRebarCount90 = GetPunchingRebarCount(column, step, foundationSlab, rebarCoverDown, backRebarDiameter, true);
                double length = PunchingRebarGeometryCalculator.GetFrameLength(foundationSlab, step, rebarCoverDown, backRebarDiameter);
                double afterColumnDistance = PunchingRebarGeometryCalculator.GetAfterColumnDistance(foundationSlab, rebarCoverDown, backRebarDiameter);

                startPoint = punchingRebar.Location;

                endPoint = new XYZ
                    (
                        startPoint.X + column.FacingOrientation.X * (0.5 * length + 0.5 * column.Length + afterColumnDistance),
                        startPoint.Y + column.FacingOrientation.Y * (0.5 * length + 0.5 * column.Length + afterColumnDistance),
                        startPoint.Z
                    );
            }
            else
            {
                int punchingRebarCount = GetPunchingRebarCount(column, step, foundationSlab, rebarCoverDown, backRebarDiameter, false);

                startPoint = punchingRebar.Location;

                endPoint = new XYZ
                    (
                        startPoint.X + column.FacingOrientation.X * 0.5 * (punchingRebarCount * (2 * step) - 2 * step),
                        startPoint.Y + column.FacingOrientation.Y * 0.5 * (punchingRebarCount * (2 * step) - 2 * step),
                        startPoint.Z
                    );
            }

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
        /// <param name="isAngle90">Условный параметр, который обознеачет направление каркаса (для каркаса с индексом 90)</param>
        static private void MoveFamilyInstanceCrossColumn(PunchingRebar punchingRebar, Element foundationSlab,  Column column, double step, double rebarCoverDown, double backRebarDiameter, bool isAngle90)
        {
            XYZ startPoint = new XYZ();
            XYZ endPoint = new XYZ();

            if (isAngle90)
            {
                int punchingRebarCount90 = GetPunchingRebarCount(column, step, foundationSlab, rebarCoverDown, backRebarDiameter, true);

                startPoint = punchingRebar.Location;
                endPoint = new XYZ
                    (
                        column.Location.X - column.FacingOrientation.Y * ((punchingRebarCount90 * (2 * step) - step) / 2 - 0.5 * step),
                        column.Location.Y + column.FacingOrientation.X * ((punchingRebarCount90 * (2 * step) - step) / 2 - 0.5 * step),
                        startPoint.Z
                    );
            }
            else
            {
                double afterColumnDistance = PunchingRebarGeometryCalculator.GetAfterColumnDistance(foundationSlab, rebarCoverDown, backRebarDiameter);
                double length = PunchingRebarGeometryCalculator.GetFrameLength(foundationSlab, step, rebarCoverDown, backRebarDiameter);

                startPoint = punchingRebar.Location;
                endPoint = new XYZ
                    (
                        column.Location.X - column.FacingOrientation.Y * (length / 2 + column.Width / 2 + afterColumnDistance),
                        column.Location.Y + column.FacingOrientation.X * (length / 2 + column.Width / 2 + afterColumnDistance),
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
            XYZ startPoint = punchingRebar.Location;
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
            XYZ locationPoint = punchingRebar.Location;
            Line axeLine = Line.CreateBound(locationPoint, new XYZ(locationPoint.X, locationPoint.Y, locationPoint.Z + 1));

            punchingRebar.FamilyInstance.Location.Rotate(axeLine, angle);
        }
    }
}
