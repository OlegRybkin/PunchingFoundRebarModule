using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System;
using RevitTools;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PunchingFoundRebarModule.Model
{
    internal class PunchingRebar
    {
        internal FamilyInstance FamilyInstance {  get; private set; }

        internal XYZ Location
        {
            get 
            { 
                XYZ point = ((LocationPoint)FamilyInstance.Location).Point;
                XYZ newLocationPoint = new XYZ
                    (
                        point.X + FamilyInstance.FacingOrientation.X * 0.5 * FamilyInstance.LookupParameter("мод_ПР_Шаг по ширине").AsDouble(),
                        point.Y + FamilyInstance.FacingOrientation.Y * 0.5 * FamilyInstance.LookupParameter("мод_ПР_Шаг по ширине").AsDouble(),
                        point.Z
                    );

                return newLocationPoint; 
            }
        }

        internal PunchingRebar(FamilySymbol familySymbol, XYZ location, Slab slab, RebarParameters rebarParameters, double longRebarDiameter) 
        {
            double addLength = 50 / 304.8; // длина выпуска продольных стрежней за крайние хомуты
            double bendLegth = 90 / 304.8; // длина отгиба хомута

            FamilyInstance = familySymbol.Document.Create.NewFamilyInstance(location, familySymbol, StructuralType.NonStructural);

            double height = slab.Thickness -
                    (rebarParameters.RebarCoverUp + rebarParameters.BackRebarDiameter) -
                    (rebarParameters.RebarCoverDown + 2 * rebarParameters.BackRebarDiameter) -
                    longRebarDiameter;

            int stirrupCount = Convert.ToInt32(PunchingRebarGeometryCalculator.GetFrameLength(slab, rebarParameters) / rebarParameters.StirrupStep) + 1;

            // Заполняем параметры каркаса
            FamilyInstance.LookupParameter("обр_ПР_Код металлопроката").Set(501);
            FamilyInstance.LookupParameter("обр_Х_Код металлопроката").Set(rebarParameters.RebarClass);

            FamilyInstance.LookupParameter("мод_ПР_Шаг по ширине").Set(rebarParameters.FrameWidth - rebarParameters.RebarDiameter - longRebarDiameter);
            FamilyInstance.LookupParameter("мод_ПР_Шаг по высоте").Set(height);

            FamilyInstance.LookupParameter("мод_ПР_Диаметр_Верх").Set(longRebarDiameter);
            FamilyInstance.LookupParameter("мод_ПР_Диаметр_Низ").Set(longRebarDiameter);

            FamilyInstance.LookupParameter("мод_ПР_Анкеровка_Верх_1").Set(addLength);
            FamilyInstance.LookupParameter("мод_ПР_Анкеровка_Верх_2").Set(addLength);
            FamilyInstance.LookupParameter("мод_ПР_Анкеровка_Низ_1").Set(addLength);
            FamilyInstance.LookupParameter("мод_ПР_Анкеровка_Низ_2").Set(addLength);

            FamilyInstance.LookupParameter("мод_Х_Изменить привязку").Set(1);

            FamilyInstance.LookupParameter("мод_Х_Диаметр").Set(rebarParameters.RebarDiameter);
            FamilyInstance.LookupParameter("мод_Х_Длина отгибов").Set(bendLegth);
            FamilyInstance.LookupParameter("мод_Х_Шаг").Set(rebarParameters.StirrupStep);
            FamilyInstance.LookupParameter("мод_Х_Количество").Set(stirrupCount);

            string groupKR;

            if (slab.SlabType == SlabType.Foundation)
            {
                groupKR = "ФП_Каркасы_Продавливание";
            }
            else
            {
                groupKR = "ПП_Каркасы_Продавливание";
            }

            RevitModel.CopyParameters(slab.Element, FamilyInstance, groupKR);
        }

    }
}
