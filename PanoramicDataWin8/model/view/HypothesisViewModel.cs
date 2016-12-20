using System;
using IDEA_common.operations.risk;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{

    public class StatisticalComparisonSaveViewModel : ExtendedBindableBase
    {
        private string _filterDist0 = "";
        private string _filterDist1 = "";
        private string _saveJson = "";


        public string FilterDist0
        {
            get { return _filterDist0; }
            set { SetProperty(ref _filterDist0, value); }
        }

        public string FilterDist1
        {
            get { return _filterDist1; }
            set { SetProperty(ref _filterDist1, value); }
        }

        public string SaveJson
        {
            get { return _saveJson; }
            set { SetProperty(ref _saveJson, value); }
        }
    }


    public class HypothesisViewModel : ExtendedBindableBase
    {
        public static double DefaultSize = 15;
        public static double ExpandedHeigth = 50;
        public static double ExpandedWidth = 200;
        private static readonly Random random = new Random();
        private static int nextId = 0;

        private double _dampingFactor;

        private Pt _deltaTargetPosition = new Pt(0, 0);

        private Decision _decision;

        private bool _isDisplayed;

        private bool _isExpanded;

        private Pt _position = new Pt(0, 0);

        private Vec _size = new Vec(DefaultSize, DefaultSize);

        private Pt _targetPosition = new Pt(0, 0);

        private Vec _targetSize = new Vec(DefaultSize, DefaultSize);

        private int _viewOrdering = -1;

        private StatisticalComparisonSaveViewModel _statisticalComparisonSaveViewModel = null;


        public HypothesisViewModel()
        {
            _dampingFactor = random.NextDouble()*3.0 + 3;
        }

        public Decision Decision
        {
            get { return _decision; }
            set { SetProperty(ref _decision, value); }
        }


        public StatisticalComparisonSaveViewModel StatisticalComparisonSaveViewModel
        {
            get { return _statisticalComparisonSaveViewModel; }
            set { SetProperty(ref _statisticalComparisonSaveViewModel, value); }
        }

        public Vec TargetSize
        {
            get { return _targetSize; }
            set { SetProperty(ref _targetSize, value); }
        }

        public int ViewOrdering
        {
            get { return _viewOrdering; }
            set { SetProperty(ref _viewOrdering, value); }
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

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }
    }
}