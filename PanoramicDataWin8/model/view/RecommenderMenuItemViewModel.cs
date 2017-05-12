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

        private AttachmentViewModel _attachmentViewModel;

        public AttachmentViewModel AttachmentViewModel
        {
            get { return _attachmentViewModel; }
            set { SetProperty(ref _attachmentViewModel, value); }
        }

        private ObservableCollection<AttributeModel> _include = new ObservableCollection<AttributeModel>();

        public ObservableCollection<AttributeModel> Include
        {
            get { return _include; }
            set { SetProperty(ref _include, value); }
        }

        private ObservableCollection<AttributeModel> _exclude = new ObservableCollection<AttributeModel>();

        public ObservableCollection<AttributeModel> Exlude
        {
            get { return _exclude; }
            set { SetProperty(ref _exclude, value); }
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