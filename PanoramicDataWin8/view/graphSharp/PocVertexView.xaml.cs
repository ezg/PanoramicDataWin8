using GraphSharp.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GraphSharpSampleCore
{
    public sealed partial class PocVertexView : UserControl
    {
        public static readonly DependencyProperty BBrushProperty =
            DependencyProperty.Register("BBrush", typeof(Brush), typeof(PocVertexView), new PropertyMetadata(null));// bcz: new UIPropertyMetadata( null ) );
        public static readonly DependencyProperty BThicknessProperty =
            DependencyProperty.Register("BThickness", typeof(Thickness), typeof(PocVertexView), new PropertyMetadata(null));// bcz: new UIPropertyMetadata( null ) );
        public static readonly DependencyProperty NodeParametersProperty =
            DependencyProperty.Register("NodeParameters", typeof(List<string>), typeof(PocVertexView), new PropertyMetadata(null));// bcz: new UIPropertyMetadata( null ) );
        public static readonly DependencyProperty NodeOutputsProperty =
            DependencyProperty.Register("NodeOutputs", typeof(string), typeof(PocVertexView), new PropertyMetadata(null));// bcz: new UIPropertyMetadata( null ) );
        public static readonly DependencyProperty NodeNameProperty =
            DependencyProperty.Register("NodeName", typeof(string), typeof(PocVertexView), new PropertyMetadata(null));// bcz: new UIPropertyMetadata( null ) );
        VertexControl VC => VisualTreeHelper.GetParent(CP) as VertexControl;
        ContentPresenter CP => VisualTreeHelper.GetParent(this) as ContentPresenter;
        public PocVertexView()
        {
            this.InitializeComponent();
        }
        

        public Brush BBrush
        {
            get { return (Brush)GetValue(BBrushProperty); }
            set { SetValue(BBrushProperty, value); }
        }
        public Thickness BThickness
        {
            get { return (Thickness)GetValue(BThicknessProperty); }
            set { SetValue(BThicknessProperty, value); }
        }
        public string NodeName
        {
            get { return (string)GetValue(NodeNameProperty); }
            set { SetValue(NodeNameProperty, value); }
        }
        public List<string> NodeParameters
        {
            get { return (List<string>)GetValue(NodeParametersProperty); }
            set { SetValue(NodeParametersProperty, value); }
        }
        public string NodeOutputs
        {
            get { return (string)GetValue(NodeOutputsProperty); }
            set { SetValue(NodeOutputsProperty, value); }
        }

        Point delta = new Point();
        void MyVertex_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerMoved    -= MyVertex_PointerMoved;
            PointerMoved    += MyVertex_PointerMoved;
            PointerReleased -= MyVertex_PointerReleased;
            PointerReleased += MyVertex_PointerReleased;
            var pos = e.GetCurrentPoint(VisualTreeHelper.GetParent(VC) as UIElement).Position;
            delta = new Point(pos.X - Canvas.GetLeft(VC), pos.Y - Canvas.GetTop(VC));
            e.Handled = true;
            this.CapturePointer(e.Pointer);
        }

        private void MyVertex_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerReleased -= MyVertex_PointerReleased;
            PointerMoved -= MyVertex_PointerMoved;
            e.Handled = true;
            this.ReleasePointerCapture(e.Pointer);
        }

        private void MyVertex_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pos = e.GetCurrentPoint(VisualTreeHelper.GetParent(VC) as UIElement).Position;
            Canvas.SetLeft(VC, pos.X-delta.X);
            Canvas.SetTop(VC, pos.Y-delta.Y);
            e.Handled = true;
        }
        
        private void Parameters_Click(object sender, RoutedEventArgs e)
        {
            vFlyyout.ItemsSource = NodeParameters;
            vFlyyout.Height = NodeParameters.Count * 50;
        }
    }
}
