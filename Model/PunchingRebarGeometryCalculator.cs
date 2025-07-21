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
        static internal double GetWorkingHeight(Slab slab, RebarParameters rebarParameters)
        {
            double slabHeight = slab.Thickness;
            double workingHeight = slabHeight - (rebarParameters.RebarCoverDown + rebarParameters.BackRebarDiameter);

            return workingHeight;
        }

        /// <summary>
        /// Находит расстояние от грани колонны до первого стержня каркаса
        /// </summary>
        /// <returns></returns>
        static internal double GetAfterColumnDistance(Slab slab, RebarParameters rebarParameters)
        {
            double workingHeight = GetWorkingHeight(slab, rebarParameters);
            double afterColumnDistance = workingHeight / 3;
            double afterColumnDistanceRounded = Math.Ceiling((afterColumnDistance * 304.8) / 10) * 10 / 304.8;

            return afterColumnDistanceRounded;
        }

        /// <summary>
        /// Находит размер зоны продавливания (расстояние от грани пилона до 1,5h0)
        /// </summary>
        /// <returns></returns>
        static internal double GetPunchingZone(Slab slab, RebarParameters rebarParameters)
        {
            double workingHeight = GetWorkingHeight(slab, rebarParameters);
            double punchingZone = 1.5 * workingHeight;

            return punchingZone;
        }

        /// <summary>
        /// Находит длину каркаса (без учета "хвостиков")
        /// </summary>
        /// <returns></returns>
        static internal double GetFrameLength(Slab slab, RebarParameters rebarParameters)
        {
            double workingHeight = GetWorkingHeight(slab, rebarParameters);
            double afterColumnDistance = GetAfterColumnDistance(slab, rebarParameters);
            double punchingZoneLength = GetPunchingZone(slab, rebarParameters);
            double punchingZoneLengthRounded = Math.Ceiling(punchingZoneLength * 304.8 / 10) * 10 / 304.8;
            double frameLength = Math.Ceiling((punchingZoneLengthRounded - afterColumnDistance) / rebarParameters.StirrupStep) * rebarParameters.StirrupStep;

            return frameLength;
        }
    }
}
