using Prism.Commands;
using Prism.Mvvm;
using System.Windows;

namespace PunchingFoundRebarModule.ViewModel
{
    public class ColumnSelectWindowVM : BindableBase
    {
        internal bool IsColumnInModel { get; private set; }

        public DelegateCommand<Window> SelectFromModelBtnCommand { get; private set; }
        public DelegateCommand<Window> SelectFromLinkBtnCommand { get; private set; }

        public ColumnSelectWindowVM() 
        {
            IsColumnInModel = true;

            SelectFromModelBtnCommand = new DelegateCommand<Window>(SelectFromModelBtnFunc);
            SelectFromLinkBtnCommand = new DelegateCommand<Window>(SelectFromLinkBtnFunc);
        }

        private void SelectFromModelBtnFunc(Window window)
        {
            IsColumnInModel = true;
            window.DialogResult = true;
            window.Close();
        }

        private void SelectFromLinkBtnFunc(Window window)
        {
            IsColumnInModel = false;
            window.DialogResult = true;
            window.Close();
        }


    }
}
