using Autodesk.Revit.DB;
using Prism.Commands;
using Prism.Mvvm;
using RevitTools;
using RevitTools.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;


namespace PunchingFoundRebarModule.ViewModel
{
    public class MainWindowVM : BindableBase
    {
        //АРМАТУРА КАРКАСОВ
        private string familyName;
        public string FamilyName
        {
            get { return familyName; }
            set 
            { 
                familyName = value;
                RaisePropertyChanged(nameof(FamilyName));
            }
        }

        private string familyType;
        public string FamilyType
        {
            get { return familyType; }
            set 
            { 
                familyType = value; 
                RaisePropertyChanged(nameof(FamilyType));
            }
        }

        public List<int> RebarDiametersList { get; private set; }
        
        private int rebarDiameter;
        public int RebarDiameter
        {
            get { return rebarDiameter; }
            set 
            { 
                rebarDiameter = value; 
                RaisePropertyChanged(nameof(RebarDiameter));
            }
        }

        public Dictionary<string, int> RebarClasses { get; private set; }

        private int rebarClass;
        public int RebarClass
        {
            get { return rebarClass; }
            set 
            { 
                rebarClass = value;
                RaisePropertyChanged(nameof(RebarClass));
            }
        }

        private int stirrupStep;
        public int StirrupStep
        {
            get { return stirrupStep; }
            set 
            {   
                stirrupStep = value;
                RaisePropertyChanged(nameof(StirrupStep));
            }
        }

        private int frameWidth;
        public int FrameWidth
        {
            get { return frameWidth; }
            set
            {
                frameWidth = value;
                RaisePropertyChanged(nameof(FrameWidth));
            }
        }

        //АРМАТУРА ПЛИТЫ
        private int backRebarDiameter;
        public int BackRebarDiameter
        {
            get { return backRebarDiameter; }
            set
            {
                backRebarDiameter = value;
                RaisePropertyChanged(nameof(BackRebarDiameter));
            }
        }

        private bool isRebarCoverFromModel;
        public bool IsRebarCoverFromModel
        {
            get { return isRebarCoverFromModel; }
            set 
            { 
                isRebarCoverFromModel = value;
                IsControlEnabled = !value;
                RaisePropertyChanged(nameof(IsRebarCoverFromModel));
            }
        }

        private bool isRebarCoverFromUser;
        public bool IsRebarCoverFromUser
        {
            get { return isRebarCoverFromUser; }
            set
            {
                isRebarCoverFromUser = value;
                RaisePropertyChanged(nameof(IsRebarCoverFromUser));
            }
        }

        private bool isControlEnabled;
        public bool IsControlEnabled
        {
            get { return isControlEnabled; }
            set 
            { 
                isControlEnabled = value; 
                RaisePropertyChanged(nameof(IsControlEnabled));
            }
        }

        private int rebarCoverUp;
        public int RebarCoverUp
        {
            get { return rebarCoverUp; }
            set 
            { 
                rebarCoverUp = value;
                RaisePropertyChanged(nameof(RebarCoverUp));
            }
        }

        private int rebarCoverDown;
        public int RebarCoverDown
        {
            get { return rebarCoverDown; }
            set
            {
                rebarCoverDown = value;
                RaisePropertyChanged(nameof(RebarCoverDown));
            }
        }

        private string SettingsPath { get; set; }

        public DelegateCommand<Window> OkBtnCommand { get; private set; }
        public DelegateCommand<Window> CancelBtnCommand { get; private set; }

        public MainWindowVM(Document doc)
        { 
            RebarDiametersList = StructConstants.RebarDiameters;
            RebarClasses = StructConstants.RebarClasses;
            SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "A101 Mod", "2022", $"{doc.Title}_PunchingFoundRebar.json");

            if (File.Exists(SettingsPath))
            {
                SettingsStore settings = SettingsStore.Load(SettingsPath);

                FamilyName = settings.Get("FamilyName", "abc");
                FamilyType = settings.Get("FamilyType", "abc");

                RebarDiameter = settings.Get("RebarDiameter", 1);
                RebarClass = settings.Get("RebarClass", 1);
                StirrupStep = settings.Get("StirrupStep", 1);
                FrameWidth = settings.Get("FrameWidth", 1);

                BackRebarDiameter = settings.Get("BackRebarDiameter", 1);
                IsRebarCoverFromModel = settings.Get("IsRebarCoverFromModel", true);
                RebarCoverUp = settings.Get("RebarCoverUp", 1);
                RebarCoverDown = settings.Get("RebarCoverDown", 1);

                if (!IsRebarCoverFromModel) IsRebarCoverFromUser = true;
            }
            else
            {
                FamilyName = "IFC_Каркас_!КГор_№1";
                FamilyType = "Х_1501";

                RebarDiameter = 10;
                RebarClass = 501;
                StirrupStep = 200;
                FrameWidth = 200;

                BackRebarDiameter = 16;
                IsRebarCoverFromModel = true;
                RebarCoverUp = 20;
                RebarCoverDown = 40;
            }

            OkBtnCommand = new DelegateCommand<Window>(OkBtnFunc);
            CancelBtnCommand = new DelegateCommand<Window>(CancelBtnFunc);
        }

        private void OkBtnFunc(Window window)
        {
            SaveSettings();
            window.DialogResult = true;
            window.Close();
        }

        private void CancelBtnFunc(Window window)
        {
            window.DialogResult = false;
            window.Close();
        }

        private void SaveSettings()
        {
            SettingsStore settings = new SettingsStore();

            settings.Set("FamilyName", FamilyName);
            settings.Set("FamilyType", FamilyType);

            settings.Set("RebarDiameter", RebarDiameter);
            settings.Set("RebarClass", RebarClass);
            settings.Set("StirrupStep", StirrupStep);
            settings.Set("FrameWidth", FrameWidth);

            settings.Set("BackRebarDiameter", BackRebarDiameter);
            settings.Set("IsRebarCoverFromModel", IsRebarCoverFromModel);
            settings.Set("RebarCoverUp", RebarCoverUp);
            settings.Set("RebarCoverDown", RebarCoverDown);

            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "A101 Mod", "2022");

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            settings.Save(SettingsPath);
        }
    }
}
