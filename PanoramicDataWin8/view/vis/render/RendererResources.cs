using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace PanoramicDataWin8.view.vis.render
{
    class RendererResources
    {
        private static Dictionary<string, Color> _groupColorLookup = new Dictionary<string, Color>();

        public static Color GetGroupingColor(string grouping)
        {
            if (!_groupColorLookup.ContainsKey(grouping))
            {
                _groupColorLookup.Add(grouping,
                    SERIES_COLORS[_groupColorLookup.Count % RendererResources.SERIES_COLORS.Length]);
            }
            return _groupColorLookup[grouping];
        }

        public static Color[] SERIES_COLORS = new Color[] {
            Color.FromArgb(255, 26, 188, 156),
            Color.FromArgb(255, 243, 156, 18),
            Color.FromArgb(255, 52, 152, 219),
            Color.FromArgb(255, 52, 73, 94),
            Color.FromArgb(255, 142, 68, 173),
            Color.FromArgb(255, 241, 196, 15),
            Color.FromArgb(255, 231, 76, 60),
            Color.FromArgb(255, 149, 165, 166),
            Color.FromArgb(255, 211, 84, 0),
            Color.FromArgb(255, 189, 195, 199),
            Color.FromArgb(255, 46, 204, 113),
            Color.FromArgb(255, 155, 89, 182),
            Color.FromArgb(255, 22, 160, 133),
            Color.FromArgb(255, 41, 128, 185),
            Color.FromArgb(255, 44, 62, 80),
            Color.FromArgb(255, 230, 126, 34),
            Color.FromArgb(255, 39, 174, 96),
            Color.FromArgb(255, 127, 140, 141),
            Color.FromArgb(255, 192, 57, 43)
        }.Reverse().ToArray();
    }
}
