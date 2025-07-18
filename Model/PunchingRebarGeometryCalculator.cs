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
        static internal double GetWorkingHeight(Element foundationSlab, RebarParameters rebarParameters)
        {
            double foundationSlabHeight = GetFoundationSlabHeight(foundationSlab);
            double workingHeight = foundationSlabHeight - (rebarParameters.RebarCoverDown + rebarParameters.BackRebarDiameter);

            return workingHeight;
        }

        /// <summary>
        /// Находит расстояние от грани колонны до первого стержня каркаса
        /// </summary>
        /// <returns></returns>
        static internal double GetAfterColumnDistance(Element foundationSlab, RebarParameters rebarParameters)
        {
            double workingHeight = GetWorkingHeight(foundationSlab, rebarParameters);
            double afterColumnDistance = workingHeight / 3;
            double afterColumnDistanceRounded = Math.Ceiling((afterColumnDistance * 304.8) / 10) * 10 / 304.8;

            return afterColumnDistanceRounded;
        }

        /// <summary>
        /// Находит размер зоны продавливания (расстояние от грани пилона до 1,5h0)
        /// </summary>
        /// <param name="foundationSlab"></param>
        /// <returns></returns>
        static internal double GetPunchingZone(Element foundationSlab, RebarParameters rebarParameters)
        {
            double workingHeight = GetWorkingHeight(foundationSlab, rebarParameters);
            double punchingZone = 1.5 * workingHeight;

            return punchingZone;
        }

        /// <summary>
        /// Находит длину каркаса (без учета "хвостиков")
        /// </summary>
        /// <param name="foundationSlab"></param>
        /// <returns></returns>
        static internal double GetFrameLength(Element foundationSlab, RebarParameters rebarParameters)
        {
            double workingHeight = GetWorkingHeight(foundationSlab, rebarParameters);
            double afterColumnDistance = GetAfterColumnDistance(foundationSlab, rebarParameters);
            double punchingZoneLength = GetPunchingZone(foundationSlab, rebarParameters);
            double punchingZoneLengthRounded = Math.Ceiling(punchingZoneLength * 304.8 / 10) * 10 / 304.8;
            double frameLength = Math.Ceiling((punchingZoneLengthRounded - afterColumnDistance) / rebarParameters.StirrupStep) * rebarParameters.StirrupStep;

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
