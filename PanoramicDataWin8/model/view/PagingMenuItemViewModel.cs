using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class PagingMenuItemViewModel : MenuItemComponentViewModel
    {
        public delegate void PagingHandler(object sender, PagingDirection pagingDirection);
        
        private PagingDirection _pagingDirection;

        public PagingDirection PagingDirection
        {
            get { return _pagingDirection; }
            set { SetProperty(ref _pagingDirection, value); }
        }

        public event PagingHandler PagingEvent;

        public void FireCreateRecommendationEvent()
        {
            PagingEvent?.Invoke(this, this.PagingDirection);
        }


    }

    public enum PagingDirection
    {
        Left,
        Right
    }
}