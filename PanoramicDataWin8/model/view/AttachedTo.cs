using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class AttachedTo : ExtendedBindableBase
    {
        private Pt _position = new Pt(0, 0);

        private Vec _size = new Vec(50, 50);

        private Pt _targetPosition = new Pt(0, 0);

        public Pt TargetPosition
        {
            get { return _targetPosition; }
            set { SetProperty(ref _targetPosition, value); }
        }

        public Vec Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public Pt Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }
    }
}