using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view.tilemenu
{
    public class TileMenuContentViewModel : ExtendedBindableBase
    {
        private string _name = "";

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }
}