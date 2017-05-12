using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class RecommenderMenuItemViewModel : MenuItemComponentViewModel
    {
        public delegate void CreateRecommendationHandler(object sender, Rct bounds);

        private AttachmentViewModel _attachmentViewMode;

        public AttachmentViewModel AttachmentViewModel
        {
            get { return _attachmentViewMode; }
            set { SetProperty(ref _attachmentViewMode, value); }
        }

        public event CreateRecommendationHandler CreateRecommendationEvent;

        public void FireCreateRecommendationEvent(Rct bounds)
        {
            CreateRecommendationEvent?.Invoke(this, bounds);
        }
    }
}