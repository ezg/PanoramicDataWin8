using Windows.Foundation;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class RecommenderMenuItemViewModel : MenuItemComponentViewModel
    {
        public delegate void CreateRecommendationHandler(object sender, Rct bounds);
        public event CreateRecommendationHandler CreateRecommendationEvent;

        public void FireCreateRecommendationEvent(Rct bounds)
        {
            CreateRecommendationEvent?.Invoke(this, bounds);
        }
    }
}