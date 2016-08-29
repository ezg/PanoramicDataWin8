using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data
{
    public abstract class InputModel : ExtendedBindableBase
    {
        private OriginModel _originModel = null;
        public OriginModel OriginModel
        {
            get { return _originModel; }
            set { this.SetProperty(ref _originModel, value); }
        }

        private bool _isDisplayed = true;
        public bool IsDisplayed
        {
            get { return _isDisplayed; }
            set { this.SetProperty(ref _isDisplayed, value); }
        }

        public abstract string RawName { get; set; }
        public abstract string DisplayName { get; set; }
    }
}