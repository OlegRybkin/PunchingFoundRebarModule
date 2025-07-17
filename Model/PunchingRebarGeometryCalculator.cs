using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PunchingFoundRebarModule.Model
{
    internal class PunchingRebarGeometryCalculator
    {
        /// <summary>
        /// Находит рабочую высоту сечения фундамента (h0)
        /// </summary>
        /// <returns></returns>
        static internal double GetWorkingHeight(Element foundationSlab, double rebarCoverDown, double backRebarDiameter)
        {
            double foundationSlabHeight = GetFoundationSlabHeight(foundationSlab);
            double workingHeight = foundationSlabHeight - (rebarCoverDown + backRebarDiameter);

            return workingHeight;
        }

        /// <summary>
        /// Находит расстояние от грани колонны до первого стержня каркаса
        /// </summary>
        /// <returns></returns>
        static internal double GetAfterColumnDistance(Element foundationSlab, double rebarCoverDown, double backRebarDiameter)
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
        static internal double GetPunchingZone(Element foundationSlab, double rebarCoverDown, double backRebarDiameter)
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
        static internal double GetFrameLength(Element foundationSlab, double stirrupStep, double rebarCoverDown, double backRebarDiameter)
        {
            double workingHeight = GetWorkingHeight(foundationSlab, rebarCoverDown, backRebarDiameter);
            double afterColumnDistance = GetAfterColumnDistance(foundationSlab, rebarCoverDown, backRebarDiameter);
            double punchingZoneLength = GetPunchingZone(foundationSlab, rebarCoverDown, backRebarDiameter);
            double punchingZoneLengthRounded = Math.Ceiling(punchingZoneLength * 304.8 / 10) * 10 / 304.8;
            double frameLength = Math.Ceiling((punchingZoneLengthRounded - afterColumnDistance) / stirrupStep) * stirrupStep;

            return frameLength;
        }

        /// <summary>
        /// Определяет толщину фундаментной плиты
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        static internal double GetFoundationSlabHeight(Element element)
        {
            double foundationSlabHeight = element.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();

            return foundationSlabHeight;
        }
    }
}
