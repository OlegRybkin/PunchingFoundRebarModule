using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using PunchingFoundRebarModule.View;
using PunchingFoundRebarModule.ViewModel;
using RevitTools;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PunchingFoundRebarModule.Model
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class PunchingFoundRebar : IExternalCommand
    {
        private double RebarCoverUp {  get; set; }
        private double RebarCoverDown { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                ColumnSelectWindowVM columnSelectWindowVM = new ColumnSelectWindowVM();
                ColumnSelectWindow columnSelectWindow = new ColumnSelectWindow()
                {
                    DataContext = columnSelectWindowVM
                };

                bool? columnSelectWindowResult = columnSelectWindow.ShowDialog();
                
                if (columnSelectWindowResult != true)
                {
                    return Result.Cancelled;
                }

                //Выбор пилонов, под которыми будут размещаться каркасы
                List<Column> columns = new List<Column>();

                if (columnSelectWindowVM.IsColumnInModel)
                {
                    IList<Reference> columnsFromModelReferences = uidoc.Selection.PickObjects(ObjectType.Element, new ColumnFromModelFilter(), "Выберите пилоны");

                    foreach (Reference reference in columnsFromModelReferences)
                    {
                        columns.Add(new Column(doc.GetElement(reference.ElementId)));
                    }
                }
                else
                {
                    IList<Reference> columnsFromLinkReferences = uidoc.Selection.PickObjects(ObjectType.LinkedElement, new ColumnFromLinkFilter(doc), "Выберите пилоны");

                    foreach (Reference reference in columnsFromLinkReferences)
                    {
                        RevitLinkInstance link = doc.GetElement(reference.ElementId) as RevitLinkInstance;
                        Document linkedDoc = link.GetLinkDocument();
                        columns.Add(new Column(linkedDoc.GetElement(reference.LinkedElementId)));
                    }
                }

                MainWindowVM mainWindowVM = new MainWindowVM(doc);
                MainWindow mainWindow = new MainWindow()
                {
                    DataContext = mainWindowVM,
                };

                bool? mainWindowResult = mainWindow.ShowDialog();
                if (mainWindowResult != true)
                {
                    return Result.Cancelled;
                }

                //Выбор фундаментной плиты, в которой будут размещаться каркасы
                Reference foundationSlabReference = uidoc.Selection.PickObject(ObjectType.Element, new FoundationSlabFilter(), "Выберите фундаментную плиту");
                Element foundationSlab = doc.GetElement(foundationSlabReference.ElementId);

                if (mainWindowVM.IsRebarCoverFromModel)
                {
                    RebarCoverType rebarCoverUpType = doc.GetElement(foundationSlab.get_Parameter(BuiltInParameter.CLEAR_COVER_TOP).AsElementId()) as RebarCoverType;
                    RebarCoverUp = rebarCoverUpType.CoverDistance;

                    RebarCoverType rebarCoverDownType = doc.GetElement(foundationSlab.get_Parameter(BuiltInParameter.CLEAR_COVER_BOTTOM).AsElementId()) as RebarCoverType;
                    RebarCoverDown = rebarCoverDownType.CoverDistance;
                }
                else
                {
                    RebarCoverUp = Calculator.FromMmToFeet(mainWindowVM.RebarCoverUp);
                    RebarCoverDown = Calculator.FromMmToFeet(mainWindowVM.RebarCoverDown);
                }

                using (Transaction trans = new Transaction(doc, "Размещение IFC-каркаса"))
                {
                    trans.Start();

                    RebarParameters rebarParameters = new RebarParameters()
                    {
                        RebarDiameter = Calculator.FromMmToFeet(mainWindowVM.RebarDiameter),
                        //RebarClass = mainWindowVM.RebarClass,
                        StirrupStep = Calculator.FromMmToFeet(mainWindowVM.StirrupStep),
                        FrameWidth = Calculator.FromMmToFeet(mainWindowVM.FrameWidth),
                        //RebarLocation
                        BackRebarDiameter = Calculator.FromMmToFeet(mainWindowVM.BackRebarDiameter),
                        IsRebarCoverFromModel = mainWindowVM.IsRebarCoverFromModel,
                        RebarCoverUp = this.RebarCoverUp,
                        RebarCoverDown= this.RebarCoverDown,



                    };

                    foreach (Column column in columns)
                    {
                        PunchingRebarPlacementService.AddPunchingRebarToFoundation(doc, foundationSlab, column, rebarParameters);
                    }

                    trans.Commit();
                }

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return Result.Failed;
            }
        }
    }
}
