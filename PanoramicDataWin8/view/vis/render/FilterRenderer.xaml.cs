using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.vis.menu;
using Windows.UI.Input;
using System.Diagnostics;
using Windows.UI.Input.Inking;
using GeoAPI.Geometries;
using IDEA_common.operations;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.view.inq;
using InkStroke = PanoramicDataWin8.view.inq.InkStroke;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class FilterRenderer : Renderer, IScribbable
    {
        private readonly Gesturizer _gesturizer = new Gesturizer();
        private readonly InkRecognizerContainer _inkRecognizerContainer = new InkRecognizerContainer();

        public FilterRenderer()
        {
            this.InitializeComponent();

            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += PlotRenderer_Loaded;

            inkableScene.IsHitTestVisible = false;
            _gesturizer.AddGesture(new EraseGesture(inkableScene));
            //inkableScene.InkCollectedEvent += InkableScene_InkCollectedEvent;
        }

        void PlotRenderer_Loaded(object sender, RoutedEventArgs e)
        {
         
        }

        public override void Dispose()
        {
            base.Dispose();
            (DataContext as FilterOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
            (DataContext as FilterOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (DataContext as FilterOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
                (DataContext as FilterOperationViewModel).OperationModel.OperationModelUpdated += OperationModelUpdated;
                (DataContext as FilterOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
                (DataContext as FilterOperationViewModel).OperationModel.PropertyChanged += OperationModel_PropertyChanged;

                
            }
        }
        void OperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FilterOperationModel operationModel = (FilterOperationModel)((OperationViewModel)DataContext).OperationModel;
           
        }
        
        void OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            if (e is VisualOperationModelUpdatedEventArgs)
            {
            }
        }
        
        public override void StartSelection(Windows.Foundation.Point point)
        {
          
        }

        public override void MoveSelection(Windows.Foundation.Point point)
        {
        }

        public override bool EndSelection()
        {
            return false;
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                return this.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            }
        }

        public bool IsDeletable
        {
            get { return false; }
        }

        public IGeometry Geometry
        {
            get
            {
                FilterOperationViewModel model = this.DataContext as FilterOperationViewModel;

                Rct bounds = new Rct(model.Position, model.Size);
                return bounds.GetPolygon();
            }
        }

        public List<IScribbable> Children
        {
            get { return new List<IScribbable>(); }
        }

        public bool Consume(InkStroke inkStroke)
        {
            var tg = MainViewController.Instance.InkableScene.TransformToVisual(this);
            var offset = tg.TransformPoint(new Point());
            var transInkStroke = inkStroke.GetTranslated(offset);

            var recognizedGestures = _gesturizer.Recognize(transInkStroke.Clone());

            foreach (var recognizedGesture in recognizedGestures.ToList())
            {
                if (recognizedGesture is HitGesture)
                {
                    var hitGesture = recognizedGesture as HitGesture;
                    foreach (var hitScribbable in hitGesture.HitScribbables)
                    {
                        if (hitScribbable is InkStroke)
                        {
                            inkableScene.Remove(hitScribbable as InkStroke);
                        }
                    }
                }
            }


            if (!transInkStroke.IsErase && !recognizedGestures.Any())
            {
                inkableScene.Add(transInkStroke);
            }
            recognize();
            return true;
        }

        private async void recognize()
        {
            var im = new InkManager();
            var b = new InkStrokeBuilder();
            var recognizedText = "";

            if (inkableScene.InkStrokes.Any())
            {
                foreach (var inkStroke in inkableScene.InkStrokes)
                {
                    var pc = new PointCollection();
                    foreach (var pt in inkStroke.Points)
                    {
                        pc.Add(pt);
                    }
                    var stroke = b.CreateStroke(pc);
                    im.AddStroke(stroke);
                }
                var result = await im.RecognizeAsync(InkRecognitionTarget.All);
                recognizedText = string.Join("\n", result[0].GetTextCandidates().ToList());
            }
            txtBlock.Text = recognizedText;
        }


        private void InkableScene_InkCollectedEvent(object sender, InkCollectedEventArgs e)
        {
            var recognizedGestures = _gesturizer.Recognize(e.InkStroke.Clone());

            foreach (var recognizedGesture in recognizedGestures.ToList())
            {
                if (recognizedGesture is HitGesture)
                {
                    var hitGesture = recognizedGesture as HitGesture;
                    foreach (var hitScribbable in hitGesture.HitScribbables)
                    {
                        if (hitScribbable is InkStroke)
                        {
                            inkableScene.Remove(hitScribbable as InkStroke);
                        }
                    }
                }
            }


            if (!e.InkStroke.IsErase && !recognizedGestures.Any())
            {

                inkableScene.Add(e.InkStroke);
            }
        }

    }
}