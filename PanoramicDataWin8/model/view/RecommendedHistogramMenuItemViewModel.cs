using IDEA_common.operations.recommender;

namespace PanoramicDataWin8.model.view
{
    public class RecommendedHistogramMenuItemViewModel : MenuItemComponentViewModel
    {
        private string _id;
        private RecommendedHistogram _recommendedHistogram;

        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public RecommendedHistogram RecommendedHistogram
        {
            get { return _recommendedHistogram; }
            set { SetProperty(ref _recommendedHistogram, value); }
        }
    }
}