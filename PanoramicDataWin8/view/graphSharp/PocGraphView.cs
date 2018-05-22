using GraphSharp.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace GraphSharp.Sample
{
    public class PocGraphView : GraphLayout<PocVertex, PocEdge, PocGraph> {

        public PocGraphView()
        {
            this.RegisterPropertyChangedCallback(LayoutParametersProperty, layoutParamsChanged);
        }

        void layoutParamsChanged(DependencyObject sender, DependencyProperty dp)
        {
            var l = this.LayoutParameters;
            if (l is GraphSharp.Algorithms.Layout.Simple.Hierarchical.EfficientSugiyamaLayoutParameters sugi)
            {
                sugi.LayerDistance  = 40;
                sugi.VertexDistance = 180;
            }
        }
    }

}