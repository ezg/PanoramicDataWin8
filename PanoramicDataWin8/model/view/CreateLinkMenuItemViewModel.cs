using Windows.Foundation;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class CreateLinkMenuItemViewModel : MenuItemComponentViewModel
    {
        public delegate void CreateLinkHandler(object sender, Rct bounds);
        public event CreateLinkHandler CreateLinkEvent;

        public void FireCreateLinkEvent(Rct bounds)
        {
            CreateLinkEvent?.Invoke(this, bounds);
        }
    }
}