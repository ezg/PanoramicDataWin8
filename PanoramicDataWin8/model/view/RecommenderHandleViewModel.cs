using Windows.Foundation;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class RecommenderHandleViewModel : ExtendedBindableBase
    {
        private Pt _startPosition = new Pt(0, 0);
        private Pt _position = new Pt(0, 0);
        private Vec _size = new Vec(50, 50);
        private double _percentage = 20;
        private double _startPercentage = 20;
        private AttachmentViewModel _attachmentViewMode = null;

        public Vec Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public AttachmentViewModel AttachmentViewModel
        {
            get { return _attachmentViewMode; }
            set { SetProperty(ref _attachmentViewMode, value); }
        }

        public Pt Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }

        public Pt StartPosition
        {
            get { return _startPosition; }
            set { SetProperty(ref _startPosition, value); }
        }

        public double Percentage
        {
            get { return _percentage; }
            set { SetProperty(ref _percentage, value); }
        }


        public double StartPercentage
        {
            get { return _startPercentage; }
            set { SetProperty(ref _startPercentage, value); }
        }
    }
}