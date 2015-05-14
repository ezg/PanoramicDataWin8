using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;

namespace PanoramicDataWin8.view.inq
{
    public interface IStroqConsumer
    {
        Color StrokeColor { get; }

        FrameworkElement Element { get; }

        void Consume(InkStroke inkStroke, List<IStroqConsumer> allConsumers);
    }
}
