using Windows.Foundation;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.view.operation;

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

        public CreateLinkMenuItemViewModel(OperationViewModel operationViewModel)
        {
            CreateLinkEvent += (sender, bounds) => FilterLinkViewController.Instance.CreateFilterLinkViewModel(operationViewModel, bounds);
        }

        public CreateLinkMenuItemViewModel()
        {

        }
    }
}