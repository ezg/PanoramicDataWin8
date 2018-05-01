using GraphSharp.Controls;
using GraphSharpSampleCore;
using Windows.UI.Xaml;

namespace GraphSharp.Sample
{
    public class PocGraphView : GraphLayout<PocVertex, PocEdge, PocGraph> {

        public PocGraphView()
        {
            this.RegisterPropertyChangedCallback(LayoutParametersProperty, layoutParamsChanged);
        }

        void layoutParamsChanged(DependencyObject sender, DependencyProperty dp) {


            var l = this.LayoutParameters;
            if (l is GraphSharp.Algorithms.Layout.Simple.Hierarchical.EfficientSugiyamaLayoutParameters sugi)
            {
                sugi.LayerDistance = 40;
                sugi.VertexDistance = 140;
            }
        }
    }

}