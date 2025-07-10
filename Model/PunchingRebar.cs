using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System;
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

        internal PunchingRebar(FamilySymbol familySymbol, XYZ location, double step, double rebarDiameter, double longRebarDiameter, double height, int stirrupCount) 
        {
            double addLength = 50 / 304.8; // длина выпуска продольных стрежней за крайние хомуты
            double bendLegth = 90 / 304.8; // длина отгиба хомута

            FamilyInstance = familySymbol.Document.Create.NewFamilyInstance(location, familySymbol, StructuralType.NonStructural);

            // Заполняем параметры каркаса
            FamilyInstance.LookupParameter("обр_ПР_Код металлопроката").Set(501);
            FamilyInstance.LookupParameter("обр_Х_Код металлопроката").Set(501);

            FamilyInstance.LookupParameter("мод_ПР_Шаг по ширине").Set(step - rebarDiameter - longRebarDiameter);
            FamilyInstance.LookupParameter("мод_ПР_Шаг по высоте").Set(height);

            FamilyInstance.LookupParameter("мод_ПР_Диаметр_Верх").Set(longRebarDiameter);
            FamilyInstance.LookupParameter("мод_ПР_Диаметр_Низ").Set(longRebarDiameter);

            FamilyInstance.LookupParameter("мод_ПР_Анкеровка_Верх_1").Set(addLength);
            FamilyInstance.LookupParameter("мод_ПР_Анкеровка_Верх_2").Set(addLength);
            FamilyInstance.LookupParameter("мод_ПР_Анкеровка_Низ_1").Set(addLength);
            FamilyInstance.LookupParameter("мод_ПР_Анкеровка_Низ_2").Set(addLength);

            FamilyInstance.LookupParameter("мод_Х_Изменить привязку").Set(1);

            FamilyInstance.LookupParameter("мод_Х_Диаметр").Set(rebarDiameter);
            FamilyInstance.LookupParameter("мод_Х_Длина отгибов").Set(bendLegth);
            FamilyInstance.LookupParameter("мод_Х_Шаг").Set(step);
            FamilyInstance.LookupParameter("мод_Х_Количество").Set(stirrupCount);
        }

    }
}
