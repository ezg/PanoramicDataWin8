using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class HypothesisViewModel : ExtendedBindableBase
    {
        public static double DefaultHeight = 50;
        public static double ExpandedWidth = 150;
        private static readonly Random random = new Random();
        private static int nextId = 0;

        private double _dampingFactor;

        private Pt _position = new Pt(0, 0);

        private Vec _size = new Vec(50, DefaultHeight);

        private Pt _targetPosition = new Pt(0, 0);

        private Pt _deltaTargetPosition = new Pt(0, 0);

        private bool _isDisplayed = false;

        private bool _isExpanded = false;

        private bool _isRejected = false;

        private Vec _targetSize = new Vec(50, DefaultHeight);
        
        private double _pValue = -1;


        public HypothesisViewModel()
        {
            _dampingFactor = random.NextDouble() * 3.0 + 3;
        }

        public Vec TargetSize
        {
            get { return _targetSize; }
            set { SetProperty(ref _targetSize, value); }
        }

        public double PValue
        {
            get { return _pValue; }
            set { SetProperty(ref _pValue, value); }
        }

        public Pt TargetPosition
        {
            get { return _targetPosition; }
            set { SetProperty(ref _targetPosition, value); }
        }

        public Pt DeltaTargetPosition
        {
            get { return _deltaTargetPosition; }
            set { SetProperty(ref _deltaTargetPosition, value); }
        }

        public Vec Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }
        
        public bool IsRejected
        {
            get { return _isRejected; }
            set { SetProperty(ref _isRejected, value); }
        }

        public Pt Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }

        public double DampingFactor
        {
            get { return _dampingFactor; }
            set { SetProperty(ref _dampingFactor, value); }
        }

        public bool IsDisplayed
        {
            get { return _isDisplayed; }
            set { SetProperty(ref _isDisplayed, value); }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }
    }
}
