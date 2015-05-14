using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PanoramicDataWin8.view.inq
{
    public class InkStrokeElement : Canvas
    {
        private static SolidColorBrush COLOR = new SolidColorBrush(Color.FromArgb(255, 17, 17, 17));

        protected InkStroke _inkStroke;
        public InkStroke InkStroke { get { return _inkStroke; } }

        public InkStrokeElement(InkStroke inkStroke)
        {
            _inkStroke = inkStroke;
            _inkStroke.Points.CollectionChanged += Points_CollectionChanged;
            Redraw();
            IsHitTestVisible = false;
        }

        void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Redraw();
        }


        protected void Redraw()
        {
            this.Children.Clear();
            this.RenderTransform = new MatrixTransform();

            Polyline pl = new Polyline();
            pl.Stroke = COLOR;
            pl.StrokeThickness = 3;
            pl.Points = new PointCollection();

            foreach (var p in _inkStroke.Points)
            {
                pl.Points.Add(p);
            }

            this.Children.Add(pl);
        }
    }
}
