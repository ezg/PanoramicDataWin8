using System.Collections.Generic;
using System.Collections.ObjectModel;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class RecommenderMenuItemViewModel : MenuItemComponentViewModel
    {
        public delegate void CreateRecommendationHandler(object sender, Rct bounds);

        private RecommenderOperationViewModel _recommenderOperationViewModel = null;

        public RecommenderOperationViewModel RecommenderOperationViewModel
        {
            get { return _recommenderOperationViewModel; }
            set { SetProperty(ref _recommenderOperationViewModel, value); }
        }

        private AttachmentViewModel _attachmentViewModel;

        public AttachmentViewModel AttachmentViewModel
        {
            get { return _attachmentViewModel; }
            set { SetProperty(ref _attachmentViewModel, value); }
        }

        public event CreateRecommendationHandler CreateRecommendationEvent;

        public void FireCreateRecommendationEvent(Rct bounds)
        {
            CreateRecommendationEvent?.Invoke(this, bounds);
        }
    }

    public class RecommenderProgressMenuItemViewModel : MenuItemComponentViewModel
    {

        private HistogramOperationViewModel _histogramOperationViewModel;

        public HistogramOperationViewModel HistogramOperationViewModel
        {
            get { return _histogramOperationViewModel; }
            set { SetProperty(ref _histogramOperationViewModel, value); }
        }

    }
}