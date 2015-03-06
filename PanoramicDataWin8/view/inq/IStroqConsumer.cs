using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI;
using Windows.UI.Xaml;

namespace PanoramicData.view.inq
{
    public interface IStroqConsumer
    {
        Color StrokeColor { get; }

        FrameworkElement Element { get; }

        void Consume(InkStroke inkStroke, List<IStroqConsumer> allConsumers);
    }
}
