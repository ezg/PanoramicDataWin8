using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Foundation;

namespace PanoramicData.utils
{
    public class MathUtil
    {
        public static double EPSILON = 0.0000001;

        public static IList<float> ResampleLinear(IList<float> values, int numSamples)
        {
            var oldSamples = values;
            float scale = numSamples / (float)oldSamples.Count();
            var newSamples = new List<float>();
            for (int j = 0; j < numSamples; ++j)
            {
                newSamples.Add(0);
            }

            float radius = scale > 1 ? 1 : 1 / scale;
            for (int i = 0; i < numSamples; ++i)
            {
                float center = i / scale + (1 - scale) / 2;
                int left = (int)Math.Ceiling(center - radius);
                int right = (int)Math.Floor(center + radius);

                float sum = 0;
                float sumWeights = 0;
                for (int k = left; k <= right; k++)
                {
                    float weight = (scale >= 1) ? 1 - Math.Abs(k - center) : 1 - Math.Abs((k - center) * scale);
                    int index = Math.Max(0, Math.Min(oldSamples.Count - 1, k));
                    sum += weight * oldSamples[index];
                    sumWeights += weight;
                }
                sum /= sumWeights;
                newSamples[i] = sum;
            }

            return newSamples;
        }

        public static float Mean(IList<float> numbers)
        {
            return numbers.Sum() / numbers.Count;
        }

        public static float Distance(Point p1, Point p2)
        {
            var p = new Point(p1.X - p2.X, p1.Y - p2.Y);
            return (float)Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        public static double Clamp(double min, double max, double val)
        {
            return Math.Max(min, Math.Min(max, val));
        }

        public static IList<float> Normalize(IList<float> data)
        {
            float min = data.Min();
            float max = data.Max() - min;

            var newData = new List<float>(data.Count);
            foreach (var f in data)
            {
                newData.Add((f - min) / max);
            }
            return newData;
        }

        public static bool IsWithinRange(double min, double max, double val)
        {
            return (val >= min && val <= max);
        }

    }
}
