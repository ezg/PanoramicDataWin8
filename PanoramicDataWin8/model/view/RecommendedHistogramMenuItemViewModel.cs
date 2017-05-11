using IDEA_common.operations.recommender;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class RecommendedHistogramMenuItemViewModel : MenuItemComponentViewModel
    {
        public delegate void DroppedEventHanlder(object sender, Rct bounds);

        private string _attributeName;
        private string _id;
        private RecommendedHistogram _recommendedHistogram;

        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public string AttributeName
        {
            get { return _attributeName; }
            set { SetProperty(ref _attributeName, value); }
        }

        public RecommendedHistogram RecommendedHistogram
        {
            get { return _recommendedHistogram; }
            set { SetProperty(ref _recommendedHistogram, value); }
        }

        public HistogramOperationViewModel HistogramOperationViewModel { get; set; }
        public event DroppedEventHanlder DroppedEvent;

        public void FireDroppedEvent(Rct bounds)
        {
            DroppedEvent?.Invoke(this, bounds);
        }
    }
}