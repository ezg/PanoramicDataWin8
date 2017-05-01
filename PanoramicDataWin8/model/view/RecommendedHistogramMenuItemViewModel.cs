namespace PanoramicDataWin8.model.view
{
    public class RecommendedHistogramMenuItemViewModel : MenuItemComponentViewModel
    {
        private string _id;

        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }
    }
}