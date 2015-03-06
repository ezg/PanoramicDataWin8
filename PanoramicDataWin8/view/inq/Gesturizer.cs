using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.view.inq
{
    public class Gesturizer
    {
        private readonly IList<IGesture> _gestures = new List<IGesture>();

        public void AddGesture(IGesture gesture)
        {
            _gestures.Add(gesture);
        }

        public IList<IGesture> Recognize(InkStroke inkStroke)
        {
            var result = new List<IGesture>();
            foreach (IGesture gesture in _gestures)
            {
                if (gesture.Recognize(inkStroke))
                {
                    result.Add(gesture);
                    return result;
                }
            }
            return result;
        }
    }
}
