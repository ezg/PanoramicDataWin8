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
        private static readonly Random random = new Random();
        private static int nextId = 0;

        private double _dampingFactor;

        private Pt _position = new Pt(0, 0);

        private Vec _size = new Vec(50, DefaultHeight);

        private Pt _targetPosition = new Pt(0, 0);

        private Pt _deltaTargetPosition = new Pt(0, 0);

        private bool _isDisplayed = false;

        private Vec _targetSize = new Vec(50, DefaultHeight);

        private String _text = nextId++.ToString();


        public HypothesisViewModel()
        {
            _dampingFactor = random.NextDouble() * 3.0 + 3;
        }

        public Vec TargetSize
        {
            get { return _targetSize; }
            set { SetProperty(ref _targetSize, value); }
        }

        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
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
    }
}
