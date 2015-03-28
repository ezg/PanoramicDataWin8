using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace PanoramicData.utils
{
    public class LABColor
    {
        // This script provides a Lab color space in addition to Unity's built in Red/Green/Blue colors.
        // Lab is based on CIE XYZ and is a color-opponent space with L for lightness and a and b for the color-opponent dimensions.
        // Lab color is designed to approximate human vision and so it aspires to perceptual uniformity.
        // The L component closely matches human perception of lightness.
        // Put LABColor.cs in a 'Plugins' folder to ensure that it is accessible to other scripts.

        private float L;
        private float A;
        private float B;

        // lightness accessors
        public float l
        {
            get { return this.L; }
            set { this.L = value; }
        }

        // a color-opponent accessor
        public float a
        {
            get { return this.A; }
            set { this.A = value; }
        }

        // b color-opponent accessor
        public float b
        {
            get { return this.B; }
            set { this.B = value; }
        }

        // constructor - takes three floats for lightness and color-opponent dimensions
        public LABColor(float l, float a, float b)
        {
            this.l = l;
            this.a = a;
            this.b = b;
        }

        // constructor - takes a Color
        public LABColor(Color col)
        {
            LABColor temp = FromColor(col);
            l = temp.l;
            a = temp.a;
            b = temp.b;
        }

        // static function for linear interpolation between two LABColors
        public static LABColor Lerp(LABColor a, LABColor b, float t)
        {
            return new LABColor(LABColor.Lerp(a.l, b.l, t), LABColor.Lerp(a.a, b.a, t), LABColor.Lerp(a.b, b.b, t));
        }

        public static float Lerp(float a, float b, float percent)
        {
            return a + percent * (b - a);
        }


        // static function for interpolation between two Unity Colors through normalized colorspace
        public static Color Lerp(Color ca, Color cb, float t)
        {
            LABColor a = LABColor.FromColor(ca);
            LABColor b = LABColor.FromColor(cb);

            float al = a.l;
            float aa = a.a;
            float ab = a.b;
            float bl = b.l - al;
            float ba = b.a - aa;
            float bb = b.b - ab;

            LABColor ret = new LABColor(al + bl * t, aa + ba * t, ab + bb * t);

            return ret.ToColor();
        }

        // static function for returning the color difference in a normalized colorspace (Delta-E)
        public static float Distance(LABColor a, LABColor b)
        {
            return (float)Math.Sqrt((float)Math.Pow((a.l - b.l), 2f) + (float)Math.Pow((a.a - b.a), 2f) + (float)Math.Pow((a.b - b.b), 2f));
        }


        public static LABColor FromColor(Color c)
        {
            float r = d3_rgb_xyz(c.R);
            float g = d3_rgb_xyz(c.G);
            float b = d3_rgb_xyz(c.B);

            float x = d3_xyz_lab((0.4124564f * r + 0.3575761f * g + 0.1804375f * b) / d3_lab_X);
            float y = d3_xyz_lab((0.2126729f * r + 0.7151522f * g + 0.0721750f * b) / d3_lab_Y);
            float z = d3_xyz_lab((0.0193339f * r + 0.1191920f * g + 0.9503041f * b) / d3_lab_Z);
            return new LABColor(116f * y - 16f, 500f * (x - y), 200f * (y - z));
        }

        private static float d3_lab_X = 0.950470f;
        private static float d3_lab_Y = 1f;
        private static float d3_lab_Z = 1.088830f;

        public static float d3_lab_xyz(float x)
        {
            return x > 0.206893034f ? x * x * x : (x - 4f / 29f) / 7.787037f;
        }

        public static float d3_xyz_rgb(float r)
        {
            return (float)Math.Round(255 * (r <= 0.00304 ? 12.92 * r : 1.055 * Math.Pow(r, 1 / 2.4) - 0.055));
        }

        public static float d3_rgb_xyz(float r)
        {
            return (r /= 255f) <= 0.04045f ? r / 12.92f : (float)Math.Pow((r + 0.055f) / 1.055f, 2.4f);
        }

        public static float d3_xyz_lab(float x)
        {
            return x > 0.008856f ? (float)Math.Pow(x, 1f / 3f) : 7.787037f * x + 4f / 29f;
        }

        // static function for converting from LABColor to Color
        public static Color ToColor(LABColor lab)
        {
            float y = (lab.l + 16f) / 116f;
            float x = y + lab.a / 500f;
            float z = y - lab.b / 200f;
            x = d3_lab_xyz(x) * d3_lab_X;
            y = d3_lab_xyz(y) * d3_lab_Y;
            z = d3_lab_xyz(z) * d3_lab_Z;


            return Color.FromArgb(
                255,
                (byte)d3_xyz_rgb(3.2404542f * x - 1.5371385f * y - 0.4985314f * z),
                (byte)d3_xyz_rgb(-0.9692660f * x + 1.8760108f * y + 0.0415560f * z),
                (byte)d3_xyz_rgb(0.0556434f * x - 0.2040259f * y + 1.0572252f * z)
            );
        }


        // function for converting an instance of LABColor to Color
        public Color ToColor()
        {
            return LABColor.ToColor(this);
        }

        // override for string
        public override string ToString()
        {
            return "L:" + l + " A:" + a + " B:" + b;
        }

        // are two LABColors the same?
        public override bool Equals(System.Object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            return (this == (LABColor)obj);
        }

        // override hashcode for a LABColor
        public override int GetHashCode()
        {
            return l.GetHashCode() ^ a.GetHashCode() ^ b.GetHashCode();
        }

        // Equality operator
        public static bool operator ==(LABColor item1, LABColor item2)
        {
            return (item1.l == item2.l && item1.a == item2.a && item1.b == item2.b);
        }

        // Inequality operator
        public static bool operator !=(LABColor item1, LABColor item2)
        {
            return (item1.l != item2.l || item1.a != item2.a || item1.b != item2.b);
        }
    }
}
