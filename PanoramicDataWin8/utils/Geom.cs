using System;
using System.Collections.Generic;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra;

namespace PanoramicDataWin8.utils
{
    /// <summary>
    /// Angle measured in degrees. Type-safety for angle measurements. See the types <c>Rad</c> and <c>Circles</c>.
    /// </summary>
    public struct Deg
    {
        public double D { get; private set; }
        // Note that for no apparent reason, the compiler (new for C# 3.0) requires calling the default constructor as an initializer for other constructors
        // in order to use these new anonymous fields.
        public Deg(int d) : this() { D = d; }
        public Deg(long d) : this() { D = d; }
        public Deg(float d) : this() { D = d; }
        public Deg(double d) : this() { D = d; }
        public Deg(Deg d) : this() { D = d.D; }
        public Deg(Rad r) : this() { D = r.R * 180 / Math.PI; }
        public Deg(Circles c) : this() { D = c.Frac * 360; }
        public static explicit operator Deg(double d) { return new Deg(d); }
        public static implicit operator Deg(Rad r) { return new Deg(r); }
        public static implicit operator Deg(Circles c) { return new Deg(c); }

        public static Deg Circle { get { return new Deg(360.0); } }

        public static Deg operator +(Deg a, Deg b) { return new Deg(a.D + b.D); }
        public static Deg operator -(Deg a, Deg b) { return new Deg(a.D - b.D); }
        public static Deg operator -(Deg a) { return new Deg(-a.D); }
        public static Deg operator *(double a, Deg b) { return new Deg(a * b.D); }
        public static Deg operator *(Deg a, double b) { return new Deg(a.D * b); }
        public static Deg operator /(Deg a, double b) { return new Deg(a.D / b); }
        public static double operator /(Deg a, Deg b) { return a.D / b.D; }
        public static double operator /(Deg a, Rad b) { return a / (Deg)b; }
        public static double operator /(Deg a, Circles b) { return a / (Deg)b; }
        public static Deg operator %(Deg a, Deg b) { return new Deg(Math.IEEERemainder(a.D, b.D)); }
        public static Deg operator %(Deg a, Rad b) { return a % (Deg)b; }
        public static Deg operator %(Deg a, Circles b) { return a % (Deg)b; }

        public static bool operator ==(Deg a, Deg b) { return a.D == b.D; }
        public static bool operator !=(Deg a, Deg b) { return a.D != b.D; }
        public static bool operator ==(Deg a, Rad b) { return a == (Deg)b; }
        public static bool operator !=(Deg a, Rad b) { return a != (Deg)b; }
        public static bool operator ==(Deg a, Circles b) { return a == (Deg)b; }
        public static bool operator !=(Deg a, Circles b) { return a != (Deg)b; }
        public override bool Equals(object obj) { Deg? d = obj as Deg?; return d.HasValue && this == d.Value; }
        public override int GetHashCode() { return D.GetHashCode(); }
        public override string ToString() { return "{" + D + " degrees}"; }

        public static bool operator <(Deg a, Deg b) { return a.D < b.D; }
        public static bool operator <=(Deg a, Deg b) { return a.D <= b.D; }
        public static bool operator >=(Deg a, Deg b) { return a.D >= b.D; }
        public static bool operator >(Deg a, Deg b) { return a.D > b.D; }
        public static bool operator <(Deg a, Rad b) { return a < (Deg)b; }
        public static bool operator <=(Deg a, Rad b) { return a <= (Deg)b; }
        public static bool operator >=(Deg a, Rad b) { return a >= (Deg)b; }
        public static bool operator >(Deg a, Rad b) { return a > (Deg)b; }
        public static bool operator <(Deg a, Circles b) { return a < (Deg)b; }
        public static bool operator <=(Deg a, Circles b) { return a <= (Deg)b; }
        public static bool operator >=(Deg a, Circles b) { return a >= (Deg)b; }
        public static bool operator >(Deg a, Circles b) { return a > (Deg)b; }
    }
    /// <summary>
    /// Angle measured in radians. Type-safety for angle measurements. See the types <c>Deg</c> and <c>Circles</c>.
    /// </summary>
    public struct Rad
    {
        public double R { get; private set; }
        // Note that for no apparent reason, the compiler (new for C# 3.0) requires calling the default constructor as an initializer for other constructors
        // in order to use these new anonymous fields.
        public Rad(long r) : this() { R = r; }
        public Rad(int r) : this() { R = r; }
        public Rad(float r) : this() { R = r; }
        public Rad(double r) : this() { R = r; }
        public Rad(Deg d) : this() { R = d.D * Math.PI / 180; }
        public Rad(Rad r) : this() { R = r.R; }
        public Rad(Circles c) : this() { R = c.Frac * 2 * Math.PI; }
        public static implicit operator Rad(double d) { return new Rad(d); }
        public static implicit operator double(Rad r) { return r.R; }
        public static implicit operator Rad(Deg d) { return new Rad(d); }
        public static implicit operator Rad(Circles c) { return new Rad(c); }

        public static Rad Pi { get { return new Rad(Math.PI); } }
        public static Rad Circle { get { return new Rad(2 * Math.PI); } }

        public static Rad operator +(Rad a, Rad b) { return new Rad(a.R + b.R); }
        public static Rad operator -(Rad a, Rad b) { return new Rad(a.R - b.R); }
        public static Rad operator -(Rad a) { return new Rad(-a.R); }
        public static Rad operator *(double a, Rad b) { return new Rad(a * b.R); }
        public static Rad operator *(Rad a, double b) { return new Rad(a.R * b); }
        public static Rad operator /(Rad a, double b) { return new Rad(a.R / b); }
        public static Rad operator *(int a, Rad b) { return new Rad(a * b.R); }
        public static Rad operator *(Rad a, int b) { return new Rad(a.R * b); }
        public static Rad operator /(Rad a, int b) { return new Rad(a.R / b); }
        public static double operator /(Rad a, Rad b) { return a.R / b.R; }
        public static double operator /(Rad a, Deg b) { return a / (Rad)b; }
        public static double operator /(Rad a, Circles b) { return a / (Rad)b; }
        public static Rad operator %(Rad a, Rad b) { return new Rad(Math.IEEERemainder(a.R, b.R)); }
        public static Rad operator %(Rad a, Deg b) { return a % (Rad)b; }
        public static Rad operator %(Rad a, Circles b) { return a % (Rad)b; }

        public static bool operator ==(Rad a, Rad b) { return a.R == b.R; }
        public static bool operator !=(Rad a, Rad b) { return a.R != b.R; }
        public static bool operator ==(Rad a, Deg b) { return a == (Rad)b; }
        public static bool operator !=(Rad a, Deg b) { return a != (Rad)b; }
        public static bool operator ==(Rad a, Circles b) { return a == (Rad)b; }
        public static bool operator !=(Rad a, Circles b) { return a != (Rad)b; }
        public override bool Equals(object obj) { Rad? d = obj as Rad?; return d.HasValue && this == d.Value; }
        public override int GetHashCode() { return R.GetHashCode(); }
        public override string ToString() { return "{" + R + " radians}"; }

        public static bool operator <(Rad a, Rad b) { return a.R < b.R; }
        public static bool operator <=(Rad a, Rad b) { return a.R <= b.R; }
        public static bool operator >=(Rad a, Rad b) { return a.R >= b.R; }
        public static bool operator >(Rad a, Rad b) { return a.R > b.R; }
        public static bool operator <(Rad a, Deg b) { return a < (Rad)b; }
        public static bool operator <=(Rad a, Deg b) { return a <= (Rad)b; }
        public static bool operator >=(Rad a, Deg b) { return a >= (Rad)b; }
        public static bool operator >(Rad a, Deg b) { return a > (Rad)b; }
        public static bool operator <(Rad a, Circles b) { return a < (Rad)b; }
        public static bool operator <=(Rad a, Circles b) { return a <= (Rad)b; }
        public static bool operator >=(Rad a, Circles b) { return a >= (Rad)b; }
        public static bool operator >(Rad a, Circles b) { return a > (Rad)b; }
    }
    /// <summary>
    /// Angle measured in fractions of a full circle. Ie, 1 circle is 360 degrees. Type-safety for angle measurements. See the types <c>Deg</c> and <c>Rad</c>.
    /// </summary>
    public struct Circles
    {
        public double Frac { get; private set; }
        // Note that for no apparent reason, the compiler (new for C# 3.0) requires calling the default constructor as an initializer for other constructors
        // in order to use these new anonymous fields.
        public Circles(long f) : this() { Frac = f; }
        public Circles(int f) : this() { Frac = f; }
        public Circles(float f) : this() { Frac = f; }
        public Circles(double f) : this() { Frac = f; }
        public Circles(Deg d) : this() { Frac = d.D / 360; }
        public Circles(Rad r) : this() { Frac = r.R / (2 * Math.PI); }
        public Circles(Circles c) : this() { Frac = c.Frac; }
        public static explicit operator Circles(double d) { return new Circles(d); }
        public static implicit operator Circles(Deg d) { return new Circles(d); }
        public static implicit operator Circles(Rad r) { return new Circles(r); }

        public static Circles Circle { get { return new Circles(1.0); } }

        public static Circles operator +(Circles a, Circles b) { return new Circles(a.Frac + b.Frac); }
        public static Circles operator -(Circles a, Circles b) { return new Circles(a.Frac - b.Frac); }
        public static Circles operator -(Circles a) { return new Circles(-a.Frac); }
        public static Circles operator *(double a, Circles b) { return new Circles(a * b.Frac); }
        public static Circles operator *(Circles a, double b) { return new Circles(a.Frac * b); }
        public static Circles operator /(Circles a, double b) { return new Circles(a.Frac / b); }
        public static double operator /(Circles a, Circles b) { return a.Frac / b.Frac; }
        public static double operator /(Circles a, Rad b) { return a / (Circles)b; }
        public static double operator /(Circles a, Deg b) { return a / (Circles)b; }
        public static Circles operator %(Circles a, Circles b) { return new Circles(Math.IEEERemainder(a.Frac, b.Frac)); }
        public static Circles operator %(Circles a, Deg b) { return a % (Circles)b; }
        public static Circles operator %(Circles a, Rad b) { return a % (Circles)b; }

        public static bool operator ==(Circles a, Circles b) { return a.Frac == b.Frac; }
        public static bool operator !=(Circles a, Circles b) { return a.Frac != b.Frac; }
        public static bool operator ==(Circles a, Deg b) { return a == (Circles)b; }
        public static bool operator !=(Circles a, Deg b) { return a != (Circles)b; }
        public static bool operator ==(Circles a, Rad b) { return a == (Circles)b; }
        public static bool operator !=(Circles a, Rad b) { return a != (Circles)b; }
        public override bool Equals(object obj) { Circles? d = obj as Circles?; return d.HasValue && this == d.Value; }
        public override int GetHashCode() { return Frac.GetHashCode(); }
        public override string ToString() { return "{" + Frac + " of a circle}"; }

        public static bool operator <(Circles a, Circles b) { return a.Frac < b.Frac; }
        public static bool operator <=(Circles a, Circles b) { return a.Frac <= b.Frac; }
        public static bool operator >=(Circles a, Circles b) { return a.Frac >= b.Frac; }
        public static bool operator >(Circles a, Circles b) { return a.Frac > b.Frac; }
        public static bool operator <(Circles a, Deg b) { return a < (Circles)b; }
        public static bool operator <=(Circles a, Deg b) { return a <= (Circles)b; }
        public static bool operator >=(Circles a, Deg b) { return a >= (Circles)b; }
        public static bool operator >(Circles a, Deg b) { return a > (Circles)b; }
        public static bool operator <(Circles a, Rad b) { return a < (Circles)b; }
        public static bool operator <=(Circles a, Rad b) { return a <= (Circles)b; }
        public static bool operator >=(Circles a, Rad b) { return a >= (Circles)b; }
        public static bool operator >(Circles a, Rad b) { return a > (Circles)b; }
    }
    public static class AngleUnitsExtensions
    {
        public static Deg Deg(this long n) { return new Deg(n); }
        public static Deg Deg(this int n) { return new Deg(n); }
        public static Deg Deg(this float n) { return new Deg(n); }
        public static Deg Deg(this double n) { return new Deg(n); }

        public static Rad Rad(this long n) { return new Rad(n); }
        public static Rad Rad(this int n) { return new Rad(n); }
        public static Rad Rad(this float n) { return new Rad(n); }
        public static Rad Rad(this double n) { return new Rad(n); }

        public static Circles Circles(this long n) { return new Circles((double)n); }
        public static Circles Circles(this int n) { return new Circles((double)n); }
        public static Circles Circles(this float n) { return new Circles((double)n); }
        public static Circles Circles(this double n) { return new Circles(n); }
    }
    /// <summary>
    /// A point: a fixed position in space. We assume no distinguished origin, so points are not completely equivalent to
    /// vectors, and they have different operations available.
    /// </summary>
    public struct Pt
    {
        public double X { get; set; }
        public double Y { get; set; }

        // Note that for no apparent reason, the compiler (new for C# 3.0) requires calling the default constructor as an initializer for other constructors
        // in order to use these new anonymous fields.
        public Pt(double x, double y) : this() { X = x; Y = y; }
        public Pt(Pt p) { this = p; }

        public static Pt operator +(Pt p, Vec v)
        {
            return new Pt(p.X + v.X, p.Y + v.Y);
        }
        public static Pt operator +(Vec v, Pt p)
        {
            return new Pt(v.X + p.X, v.Y + p.Y);
        }
        public static Pt operator +(Pt p, Pt p2)
        {
            return new Pt(p2.X + p.X, p2.Y + p.Y);
        }
        public static Vec operator -(Pt a, Pt b)
        {
            return new Vec(a.X - b.X, a.Y - b.Y);
        }
        public static Pt operator -(Pt p, Vec v)
        {
            return new Pt(p.X - v.X, p.Y - v.Y);
        }

        public static bool operator ==(Pt a, Pt b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(Pt a, Pt b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Pt && this == (Pt)obj;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override string ToString() { return "[" + X + ", " + Y + "]"; }

        // both ways cause can't define operators on built-in types.
        public static implicit operator Point(Pt p) { return new Point(p.X, p.Y); }
        public static implicit operator Pt(Point p) { return new Pt(p.X, p.Y); }
        public static explicit operator Size(Pt p) { return new Size(p.X, p.Y); }
        public static explicit operator Pt(Size p) { return new Pt(p.Width, p.Height); }
        public static explicit operator Pt(Vec p) { return new Pt(p.X, p.Y); }
     
        /// <summary>
        /// The centroid of the given points.
        /// </summary>
        public static Pt Avg(Pt p0, params Pt[] ps)
        {
            Vec sum = (Vec)p0;
            foreach (Pt p in ps) sum += (Vec)p;
            return (Pt)(sum / (ps.Length + 1));
        } /// <summary>
        /// The centroid of the given points.
        /// </summary>
        public static Pt Avg(params Pt[] ps)
        {
            Vec sum = new Vec();
            foreach (Pt p in ps) sum += (Vec)p;
            return (Pt)(sum / (ps.Length));
        }
    }
    /// <summary>
    /// A vector: definite direction and magnitude, but not a fixed position.
    /// </summary>
    public struct Vec
    {
        public double X { get; set; }
        public double Y { get; set; }

        // Note that for no apparent reason, the compiler (new for C# 3.0) requires calling the default constructor as an initializer for other constructors
        // in order to use these new anonymous fields.
        public Vec(double x, double y) : this() { X = x; Y = y; }
        public Vec(Vec v) { this = v; }
        /// <summary>
        /// Unit vector pointing along the positive X axis.
        /// </summary>
        static public Vec Xaxis { get { return new Vec(1, 0); } }
        /// <summary>
        /// Unit vector pointing along the positive Y axis;
        /// </summary>
        static public Vec Yaxis { get { return new Vec(0, 1); } }
        /// <summary>
        /// Unit vector pointing up on-screen.
        /// </summary>
        static public Vec Up { get { return new Vec(0, -1); } }
        /// <summary>
        /// Unit vector pointing down on-screen.
        /// </summary>
        static public Vec Down { get { return new Vec(0, 1); } }
        /// <summary>
        /// Unit vector pointing left on-screen.
        /// </summary>
        static public Vec Left { get { return new Vec(-1, 0); } }
        /// <summary>
        /// Unit vector pointing right on-screen.
        /// </summary>
        static public Vec Right { get { return new Vec(1, 0); } }

        public static Vec operator +(Vec a, Vec b)
        {
            return new Vec(a.X + b.X, a.Y + b.Y);
        }
        public static Vec operator -(Vec a, Vec b)
        {
            return new Vec(a.X - b.X, a.Y - b.Y);
        }
        public static Vec operator -(Vec v)
        {
            return new Vec(-v.X, -v.Y);
        }

        public static Vec operator *(double s, Vec v)
        {
            return new Vec(s * v.X, s * v.Y);
        }
        public static Vec operator *(Vec v, double s)
        {
            return new Vec(v.X * s, v.Y * s);
        }
        public static Vec operator /(Vec v, double s)
        {
            return new Vec(v.X / s, v.Y / s);
        }

        public static bool operator ==(Vec a, Vec b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(Vec a, Vec b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Vec && this == (Vec)obj;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        // both ways cause can't define operators on built-in types.
        public static explicit operator Point(Vec v) { return new Point(v.X, v.Y); }
        public static explicit operator Vec(Point v) { return new Vec(v.X, v.Y); }
        public static explicit operator Size(Vec v) { return new Size(v.X, v.Y); }
        public static explicit operator Vec(Size v) { return new Vec(v.Width, v.Height); }
        public static explicit operator Vec(Pt v) { return new Vec(v.X, v.Y); }

        public double Dot(Vec b) { return X * b.X + Y * b.Y; }
        public static double Dot(Vec a, Vec b) { return a.Dot(b); }
        /// <summary>
        /// Signed angle from this vector to the argument, in the range from -pi to pi. Right-handed, so angles
        /// which are CCW on the screen are negative because screen coordinates are left handed.
        /// </summary>
        public Rad SignedAngle(Vec b) { return Math.Atan2(PerpDot(b), Dot(b)); }
        /// <summary>
        /// Unsigned angle from this vector to the argument, in the range from 0 to pi.
        /// </summary>
        public Rad UnsignedAngle(Vec b) { return Math.Abs(SignedAngle(b)); }
        /// <summary>
        /// Returns normalized vector, not the normal to the vector (see Perp for that).
        /// </summary>
        public Vec Normal()
        {
            return new Vec(this) / this.Length;
        }
        /// <summary>
        /// Returns normalized vector.
        /// </summary>
        public Vec Normalized()
        {
            return new Vec(this) / this.Length;
        }
        /// <summary>
        /// Normalizes this vector in place.
        /// </summary>
        public void Normalize()
        {
            double len = this.Length;
            X /= len;
            Y /= len;
        }
        /// <summary>
        /// Related to the cross product in 3d, this returns the vector rotated by -pi/2 on the left-handed screen. (Rotated by pi/2 if coords are taken
        /// to be in some other right-handed coordinate system.)
        /// Useful to find normal directions to lines.
        /// </summary>
        public Vec Perp() { return new Vec(-Y, X); }
        /// <summary>
        /// Returns <c>ax by - ay bx</c>. This is <c>Perp().Dot(b)</c>, whose official name is
        /// perp dot, but which is also called cross (in 2D) and determinant (also in 2D).
        /// It's right-handed, so if the angle from this vector to the argument is CCW on the screen then the result will be negative because the screen
        /// is left-handed.
        /// </summary>
        public double PerpDot(Vec b) { return Perp().Dot(b); }
        /// <summary>
        /// Same as PerpDot(): Returns <c>ax by - ay bx</c>. The official name of this is
        /// perp dot, but which is also called cross (in 2D) and determinant (also in 2D).
        /// It's right-handed, so if the angle from this vector to the argument is CCW on the screen then the result will be negative because the screen
        /// is left-handed.
        /// </summary>
        public double Det(Vec b) { return PerpDot(b); }
        /// <summary>
        /// Square of the length of the vector (but computed more efficiently as the dot product of the vector with itself).
        /// </summary>
        public double Length2 { get { return Dot(this); } }
        /// <summary>
        /// For compatibility with code written for System.Windows.Vector.
        /// </summary>
        /// <returns></returns>
        public double LengthSquared { get { return Dot(this); } }
        /// <summary>
        /// Length of the vector.
        /// </summary>
        public double Length { get { return Math.Sqrt(Length2); } }

        public override string ToString() { return "" + X + ", " + Y; }
    }
    /// <summary>
    /// An affine transformation matrix.
    /// </summary>
    public struct       Mat
    {
        public override string ToString()
        {
            return "{" + _e00 + "," + _e10 + " " + _e01 + "," + _e11 + "}" + "{" + _e02 + "," + _e12 + "}";
        }
        private double _e00, _e01, _e02, _e10, _e11, _e12;
        private delegate void eSet(Mat m, double d);
        private delegate double eGet(Mat m);
        private static eSet[,] _setters;
        private static eGet[,] _getters;
        static Mat()
        {
            _setters = new eSet[2, 3];
            _setters[0, 0] = delegate(Mat m, double d) { m._e00 = d; };
            _setters[0, 1] = delegate(Mat m, double d) { m._e01 = d; };
            _setters[0, 2] = delegate(Mat m, double d) { m._e02 = d; };
            _setters[1, 0] = delegate(Mat m, double d) { m._e10 = d; };
            _setters[1, 1] = delegate(Mat m, double d) { m._e11 = d; };
            _setters[1, 2] = delegate(Mat m, double d) { m._e12 = d; };
            _getters = new eGet[2, 3];
            _getters[0, 0] = delegate(Mat m) { return m._e00; };
            _getters[0, 1] = delegate(Mat m) { return m._e01; };
            _getters[0, 2] = delegate(Mat m) { return m._e02; };
            _getters[1, 0] = delegate(Mat m) { return m._e10; };
            _getters[1, 1] = delegate(Mat m) { return m._e11; };
            _getters[1, 2] = delegate(Mat m) { return m._e12; };
        }

        static public Mat Rect(Rct r)
        {
            Mat s = Mat.Translate(r.TopLeft);
            s._e00 = r.Width;
            s._e11 = r.Height;
            return s;
        }
        static public Mat Rotate(Deg d)
        {
            return Rotate((d.D * Math.PI) / 180.0, new Pt(0,0));
        }
        static public Mat Rotate(Rad r, Pt center)
        {
            double r00 = Math.Cos(r.R);
            double r01 = -Math.Sin(r.R);
            double r10 = Math.Sin(r.R);
            double r11 = Math.Cos(r.R);

            Mat mat = new Mat();
            mat._e00 = r00;
            mat._e01 = r01;
            mat._e02 = center.X - r00 * center.X - r01 * center.Y;
            mat._e10 = r10;
            mat._e11 = r11;
            mat._e12 = center.Y - r10 * center.X - r11 * center.Y;

            return mat;
        }
        static public Mat Scale(Vec v) { return Scale(v.X, v.Y); }
        static public Mat Scale(double x, double y)
        {
            Mat s = Mat.Identity;
            s._e00 = x;
            s._e11 = y;
            return s;
        }
        static public Mat Scale(Vec v, Pt p)
        {
            return Mat.Translate(new Pt() - p) *     // compute scaling transformation about tl point
                   Mat.Scale(v) *
                   Mat.Translate(p);
        }
        static public Mat Scale(Vec v, Vec n, Pt p)
        {
            return Mat.Translate(new Pt() - p) *     // compute scaling transformation about tl point
                   Mat.Rotate(n.SignedAngle(new Vec(1, 0))) *
                   Mat.Scale(v) *
                   Mat.Rotate(-n.SignedAngle(new Vec(1, 0))) *
                   Mat.Translate(p);
        }

        static public Mat Translate(double x, double y)
        {
            Mat s = Mat.Identity;
            s._e02 = x;
            s._e12 = y;
            return s;
        }
        static public Mat Translate(Vec v) { return Translate(v.X, v.Y); }
        static public Mat Translate(Pt p) { return Translate(p.X, p.Y); }

        /// <summary>
        /// For multiplication by column vectors, i is the row and j is the column.
        /// </summary>
        public double this[int i, int j] { get { return _getters[i, j](this); } set { _setters[i, j](this, value); } }

        /// <summary>
        /// For multiplication by column vectors, the first index of the array is the row and the second the column.
        /// </summary>
        public Mat(double[,] elts)
        {
            _e00 = elts[0, 0];
            _e01 = elts[0, 1];
            _e02 = elts[0, 2];
            _e10 = elts[1, 0];
            _e11 = elts[1, 1];
            _e12 = elts[1, 2];
        }
        public Mat(Windows.UI.Xaml.Media.Matrix m)
        {
            _e00 = m.M11;
            _e01 = m.M21;
            _e02 = m.OffsetX;
            _e10 = m.M12;
            _e11 = m.M22;
            _e12 = m.OffsetY;
        }
        public Mat(Mat m) { this = m; }

        public static implicit operator Windows.UI.Xaml.Media.Matrix(Mat m)
        {
            return new Windows.UI.Xaml.Media.Matrix(m._e00, m._e10, m._e01, m._e11, m._e02, m._e12);
        }
        public static implicit operator Mat(Windows.UI.Xaml.Media.Matrix m)
        {
            return new Mat(m);
        }
        public static implicit operator Matrix<double>(Mat m)
        {
            Matrix<double> mret = Matrix<double>.Build.Dense(3, 3);
            mret[0, 0] = m._e00;
            mret[0, 1] = m._e01;
            mret[0, 2] = m._e02;
            mret[1, 0] = m._e10;
            mret[1, 1] = m._e11;
            mret[1, 2] = m._e12;
            mret[2, 0] = 0;
            mret[2, 1] = 0;
            mret[2, 2] = 1;
            return mret;
        }
        public static implicit operator Mat(Matrix<double> m)
        {
            Mat mret = Mat.Identity;
            mret._e00 = m[0, 0];
            mret._e01 = m[0, 1];
            mret._e02 = m[0, 2];
            mret._e10 = m[1, 0];
            mret._e11 = m[1, 1];
            mret._e12 = m[1, 2];
            return mret;
        }
        public static Mat operator *(Mat a, Mat b)
        {
            return (Mat)((Matrix<double>)a * (Matrix<double>)b);
        }
        public static Pt operator *(Mat m, Pt p) { return new Pt(p.X * m[0, 0] + p.Y * m[0, 1] + m[0, 2], p.X * m[1, 0] + p.Y * m[1, 1] + m[1, 2]); }
        public static Vec operator *(Mat m, Vec v) { return new Vec(v.X * m[0, 0] + v.Y * m[0, 1], v.X * m[1, 0] + v.Y * m[1, 1]); }
        public static Rct operator *(Mat m, Rct r) { return new Rct(m * r.TopLeft, m * r.BottomRight); }
        public static Mat Identity
        {
            get
            {
                Mat m = new Mat();
                m._e00 = m._e11 = 1;
                return m;
            }
        }
        /// <summary>
        /// Invert the matrix in place.
        /// </summary>
        public void Invert()
        {
            this = Inverse();
        }
        /// <summary>
        /// Return the inverse of the matrix.
        /// </summary>
        /// <returns></returns>
        public Mat Inverse()
        {
            return (Mat)((Matrix<double>)this).Inverse();
        }

        public static bool operator ==(Mat a, Mat b)
        {
            for (int i = 0; i < 2; i++) for (int j = 0; j < 3; j++) if (a[i, j] != b[i, j]) return false;
            return true;
        }
        public static bool operator !=(Mat a, Mat b) { return !(a == b); }

        public override bool Equals(object obj)
        {
            return obj is Mat && this == (Mat)obj;
        }

        public override int GetHashCode()
        {
            int hc = 0;
            for (int i = 0; i < 2; i++) for (int j = 0; j < 3; j++) hc ^= this[i, j].GetHashCode();
            return hc;
        }
    }
    /// <summary>
    /// A line (infinite, not a line segment). This tries to embody the mathematical concept, and thus has no distinguished point or length.
    /// </summary>
    public struct Ln
    {
        /// <summary>
        /// The normal vector to the line. Should be unit length.
        /// </summary>
        public Vec N { get; set; }
        /// <summary>
        /// This comes from the line equation: N . x + D = 0. (It's D because the plane equation in 3D is A x + B y + C z + D = 0, and
        /// because it's the distance the line is from the origin.)
        /// </summary>
        public double D { get; set; }

        /// <summary>
        /// Construct a line from its normal vector and distance from the origin.
        /// </summary>
        /// <param name="n">Normal vector</param>
        /// <param name="d">Distance from the origin multiplied by the length of the normal vector.</param>
        public Ln(Vec n, double d) : this() { D = d / n.Length; N = n.Normalized(); }
        /// <summary>
        /// Construct the line passing between two points. The positive side of the line on the (left-handed) screen will be to the right as you look from A at B.
        /// </summary>
        public Ln(Pt A, Pt B) : this(A, B - A) { }
        /// <summary>
        /// Construct the line through a point in a given direction. The positive side of the line on the (left-handed) screen will be to the right as you look in the
        /// direction of V.
        /// </summary>
        public Ln(Pt A, Vec V)
            : this()
        {
            N = V.Perp().Normalized();
            D = -N.Dot((Vec)A);
        }
        public Ln(Ln l) { this = l; }

        public static bool operator ==(Ln a, Ln b) { return a.N.X == b.N.X && a.N.Y == b.N.Y && a.D == b.D; }
        public static bool operator !=(Ln a, Ln b) { return !(a == b); }
        public override bool Equals(object obj) { return obj is Ln && this == (Ln)obj; }
        public override int GetHashCode() { return N.GetHashCode() ^ D.GetHashCode(); }

        public static Ln operator *(Mat mat, Ln l)
        {
            Mat m = mat.Inverse();
            return new Ln(new Vec(l.N.X * m[0, 0] + l.N.Y * m[1, 0], l.N.X * m[0, 1] + l.N.Y * m[1, 1]), l.N.X * m[1, 2] + l.N.Y * m[2, 2] + l.D);
        }
        /// <summary>
        /// The signed distance from the given point to this line. Points on the side the normal points have positive distance;
        /// those on the other side have negative distance. See the constructor comments to determine which side of the line is positive in terms of the
        /// constructor parameters; generally the right side is positive on the screen.
        /// </summary>
        public double SignedDistance(Pt a) { return N.Dot((Vec)a) + D; }
        /// <summary>
        /// The distance from the given point to this line.
        /// </summary>
        public double Distance(Pt a) { return Math.Abs(SignedDistance(a)); }
        /// <summary>
        /// Return the projection of the given point onto this line.
        /// </summary>
        public Pt ProjectPoint(Pt p) { return p - SignedDistance(p) * N; }
        /// <summary>
        /// This is the fuzz factor for various tests that might be subject to numerical precision issues.
        /// </summary>
        public static double Epsilon = 0;
        /// <summary>
        /// Does this line intersect the given one? This variant takes a fuzz factor argument determining what how close the lines have to be to parallel
        /// to be considered parallel.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="epsilon">fuzz factor: denominator can be up to the square of this value and be considered zero</param>
        /// <returns></returns>
        public bool Intersects(Ln l, double epsilon)
        {
            return Math.Abs(N.PerpDot(l.N)) > epsilon * epsilon;
        }
        /// <summary>
        /// Does this line intersect the given one?
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public bool Intersects(Ln l) { return Intersects(l, Epsilon); }
        /// <summary>
        /// Returns the intersection point of this line and the given one. If the two lines are parallel, division by zero will occur, and the returned point
        /// will have infinite coordinates.
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public Pt Intersection(Ln l)
        {
            double x = N.Y * l.D - D * l.N.Y;
            double y = D * l.N.X - N.X * l.D;
            double w = N.PerpDot(l.N);
            return new Pt(x / w, y / w);
        }
    }
    /// <summary>
    /// A line segment.
    /// </summary>
    public struct LnSeg
    {
        /// <summary>
        /// The first endpoint of the segment.
        /// </summary>
        public Pt A { get; set; }
        /// <summary>
        /// The second endpoint of the segment.
        /// </summary>
        public Pt B { get; set; }

        public LnSeg(Pt a, Pt b) : this() { A = a; B = b; }
        public LnSeg(LnSeg ls) { this = ls; }

        public static explicit operator Ln(LnSeg a) { return new Ln(a.A, a.B); }

        public static bool operator ==(LnSeg a, LnSeg b) { return a.A == b.A && a.B == b.B; }
        public static bool operator !=(LnSeg a, LnSeg b) { return !(a == b); }
        public override bool Equals(object obj) { return obj is LnSeg && this == (LnSeg)obj; }
        public override int GetHashCode() { return A.GetHashCode() ^ B.GetHashCode(); }

        public static LnSeg operator *(Mat m, LnSeg s) { return new LnSeg(m * s.A, m * s.B); }
        /// <summary>
        /// Length of the segment.
        /// </summary>
        /// <returns></returns>
        public double Length { get { return (B - A).Length; } }
        /// <summary>
        /// Square of the length of the segment.
        /// </summary>
        /// <returns></returns>
        public double Length2 { get { return (B - A).Length2; } }
        /// <summary>
        /// The midpoint of this segment.
        /// </summary>
        public Pt Center { get { return (Pt)((((Vec)A) + ((Vec)B)) / 2); } }
        /// <summary>
        /// The direction this segment points (normalized).
        /// </summary>
        /// <returns></returns>
        public Vec Direction { get { return (B - A).Normalized(); } }
        /// <summary>
        /// The axis-aligned bounding box of this segment.
        /// </summary>
        public Rct Bounds { get { return new Rct(A).Union(B); } }

        /// <summary>
        /// Project the given point onto the line this segment is part of, and return the (signed) multiple of the vector from A to B that the projected point
        /// is from A. (But more efficiently than described.)
        /// </summary>
        public double LnClosestFraction(Pt p) { return (p - A).Dot(B - A) / Length2; }
        /// <summary>
        /// Project the given point onto the line this segment is part of, and return the (signed) multiple of the vector from B to A that the projected point
        /// is from B. (But more efficiently than described.)
        /// </summary>
        public double LnClosestFractionRev(Pt p) { return (p - B).Dot(A - B) / Length2; }
        /// <summary>
        /// Return the projection of the given point onto the line this segment is part of.
        /// </summary>
        public Pt LnClosestPoint(Pt p) { return A + LnClosestFraction(p) * (B - A); }
        private double Clamp(double t) { return Math.Min(Math.Max(t, 0), 1); }
        /// <summary>
        /// Return the fraction of the way from A to B that the given point is closest to (clipping at the segment ends).
        /// </summary>
        public double ClosestFraction(Pt p) { return Clamp(LnClosestFraction(p)); }
        /// <summary>
        /// Return the fraction of the way from B to A that the given point is closest to (clipping at the segment ends).
        /// </summary>
        public double ClosestFractionRev(Pt p) { return Clamp(LnClosestFractionRev(p)); }
        /// <summary>
        /// Return the closest point in the line segment (clipping at the segment ends) to the given point.
        /// </summary>
        public Pt ClosestPoint(Pt p) { return B == A ? A : A + ClosestFraction(p) * (B - A); }
        /// <summary>
        /// Signed angle from this segment to the argument, in the range from -pi to pi. Right-handed, so angles
        /// which are CCW on the screen are negative because screen coordinates are left handed.
        /// </summary>
        public Rad SignedAngle(LnSeg b) { return (B - A).SignedAngle(b.B - b.A); }
        /// <summary>
        /// Unsigned angle from this vector to the argument, in the range from 0 to pi.
        /// </summary>
        public Rad UnsignedAngle(LnSeg b) { return (B - A).UnsignedAngle(b.B - b.A); }
        /// <summary>
        /// The intersection between this line segment and the argument. Parameters determine whether to treat the segments as rays or not and what fuzz
        /// factor to use in determining non-intersection. The return value will not have a value if there was no intersection; otherwise, it contains
        /// the intersection point. The final parameter will get the fraction of the way along this line segment the intersection is.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="epsilon">Fuzz factor: denominator can be up to the square of this value and still be considered zero. Defaults (if omitted)
        /// to <c>Ln.Epsilon</c>.</param>
        /// <param name="lines">If true, treat the line segments as lines, and return an intersection even if it lies outside the segments proper.
        /// Defaults (if omitted) to false.</param>
        /// <returns></returns>
        public Pt? Intersection(LnSeg b, double epsilon, bool lines, out double t, double expansion = 0)
        {
            Pt bA = b.A - b.Direction * expansion;
            Pt bB = b.B + b.Direction * expansion;
            Pt aA = A - Direction * expansion;
            Pt aB = B + Direction * expansion;
            t = 1;
            double d = (bB - bA).PerpDot(aB - aA);
            if (Math.Abs(d) > epsilon * epsilon)
            {
                double AB = (aB - aA).PerpDot(bA - aA) / d;
                if (lines || (AB > 0.0 && AB < 1.0))
                {
                    double CD = (bB - bA).PerpDot(bA - aA) / d;
                    if (lines || (CD > 0.0 && CD < 1.0))
                    {
                        t = CD;
                        return bA + AB * (bB - bA);
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// The intersection between this line segment and the argument. Parameters determine whether to treat the segments as rays or not and what fuzz
        /// factor to use in determining non-intersection. The return value will not have a value if there was no intersection; otherwise, it contains
        /// the intersection point.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="epsilon">Fuzz factor: denominator can be up to the square of this value and still be considered zero. Defaults (if omitted)
        /// to <c>Ln.Epsilon</c>.</param>
        /// <param name="lines">If true, treat the line segments as lines, and return an intersection even if it lies outside the segments proper.
        /// Defaults (if omitted) to false.</param>
        /// <returns></returns>
        public Pt? Intersection(LnSeg b, double epsilon, bool lines) { double t; return Intersection(b, epsilon, lines, out t); }
        /// <summary>
        /// The intersection between this line segment and the argument. The return value will not have a value if there was no intersection; otherwise,
        /// it contains the intersection point. The fuzz factor for determining intersection or not will be <c>Ln.Epsilon</c>.
        /// The final parameter will get the fraction of the way along this line segment the intersection is.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public Pt? Intersection(LnSeg b, out double t) { return Intersection(b, Ln.Epsilon, false, out t); }
        /// <summary>
        /// The intersection between this line segment and the argument. The return value will not have a value if there was no intersection; otherwise,
        /// it contains the intersection point. The fuzz factor for determining intersection or not will be <c>Ln.Epsilon</c>.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public Pt? Intersection(LnSeg b, double expansion = 0) { double t; return Intersection(b, Ln.Epsilon, false, out t, expansion); }
        /// <summary>
        /// The intersection between this line segment and the argument, each treated as lines.
        /// The return value will not have a value if there was no intersection; otherwise, it contains the intersection point. The fuzz factor for determining
        /// intersection or not will be <c>Ln.Epsilon</c>.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public Pt? LnIntersection(LnSeg b) { double t; return Intersection(b, Ln.Epsilon, true, out t); }

        public Pt? Intersection(IEnumerable<Pt> pts, out double t)
        {
            double closest = Double.PositiveInfinity;
            Pt? p = null;
            foreach (var pair in pts.ByPairs())
            {
                Pt? o;
                if ((o = Intersection(new LnSeg(pair.First, pair.Second), out t)) != null && t < closest)
                {
                    closest = t;
                    p = o;
                }
            }
            t = closest;
            if (!Double.IsPositiveInfinity(closest))
                return p;
            return null;
        }
        public Pt? Intersection(IEnumerable<Pt> pts) { double t; return Intersection(pts, out t); }

        /// <summary>
        /// The distance from the given point to this segment (*not* treated as a line).
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double Distance(Pt p) { return (p - ClosestPoint(p)).Length; }
        /// <summary>
        /// The distance from the given point to this segment, treated as a line.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double LnDistance(Pt p) { return (p - LnClosestPoint(p)).Length; }
    }
    /// <summary>
    /// A rectangle.
    /// Null rectangles are supported; a null rectangle occurs when the height or width is negative. All the methods perform correctly with null rectangles
    /// except for Center() and Inflate(). Things like TopLeft, Top, Width, Area, etc. don't work with null rectangles either (except that Width and Height
    /// will have <em>some</em> negative value).
    /// </summary>
    public struct Rct
    {
        private Pt _topLeft, _bottomRight;
        public Pt TopLeft { get { return _topLeft; } set { _topLeft = value; } }
        public Pt BottomRight { get { return _bottomRight; } set { _bottomRight = value; } }
        public Pt BottomLeft
        {
            get { return new Pt(Left, Bottom); }
            set { _topLeft.X = value.X; _bottomRight.Y = value.Y; }
        }
        public Pt TopRight
        {
            get { return new Pt(Right, Top); }
            set { _bottomRight.X = value.X; _topLeft.Y = value.Y; }
        }
        public double Top
        {
            get { return _topLeft.Y; }
            set { _topLeft.Y = value; }
        }
        public double Left
        {
            get { return _topLeft.X; }
            set { _topLeft.X = value; }
        }
        public double Bottom
        {
            get { return _bottomRight.Y; }
            set { _bottomRight.Y = value; }
        }
        public double Right
        {
            get { return _bottomRight.X; }
            set { _bottomRight.X = value; }
        }
        public double Width { get { return _bottomRight.X - _topLeft.X; } }
        public double Height { get { return _bottomRight.Y - _topLeft.Y; } }
        /// <summary>
        /// Larger of Width and Height
        /// </summary>
        public double MaxDim { get { return Math.Max(Width, Height); } }
        public double Area { get { return Width * Height; } }
        /// <summary>
        /// Diagonal from top left to bottom right--corresponds to "Size" property of .Net Rectangle
        /// </summary>
        public Vec Size { get { return _bottomRight - _topLeft; } }

        public Rct(Pt topLeft, Pt bottomRight)
            : this()
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }
        public Rct(Pt p)
            : this()
        {
            TopLeft = p;
            BottomRight = p;
        }
        public Rct(Pt topLeft, Vec size)
            : this()
        {
            TopLeft = topLeft;
            BottomRight = topLeft + size;
        }
        public Rct(double left, double top, double right, double bottom)
            : this()
        {
            TopLeft = new Pt(left, top);
            BottomRight = new Pt(right, bottom);
        }
        public Rct(Rct r) { this = r; }
        /// <summary>
        /// Used only to create a null rectangle because we can't
        /// make a null constructor for structs.
        /// </summary>
        private Rct(double dummy)
            : this()
        {
            TopLeft = new Pt(dummy, dummy);
            BottomRight = new Pt(dummy - 1, dummy - 1);
        }

        /// <summary>
        /// Return empty rectangle
        /// </summary>
        static public Rct Null
        {
            get
            {
                return new Rct(0);
            }
        }
        /// <summary>
        /// Is the rectangle empty?
        /// </summary>
        public bool IsNull()
        {
            return Top > Bottom || Left > Right;
        }

        public Rct Inflate(double width, double height)
        {
            return new Rct(Left - width / 2, Top - height / 2, Right + width / 2, Bottom + height / 2);
        }

        public bool IntersectsWith(Rct rect)
        {
            return !Intersection(rect).IsNull();
        }

        public static bool operator ==(Rct a, Rct b)
        {
            return ((a.TopLeft == b.TopLeft && a.BottomRight == b.BottomRight)
                    || (a.IsNull() && b.IsNull()));
        }
        public static bool operator !=(Rct a, Rct b)
        {
            return ((a.TopLeft != b.TopLeft || a.BottomRight != b.BottomRight)
                    && (!a.IsNull() || !b.IsNull()));
        }

        public Pt Center { get { return new Pt((Left + Right) / 2, (Top + Bottom) / 2); } }
        public Rct Translated(Vec c)
        {
            return new Rct(Left + c.X, Top + c.Y, Right + c.X, Bottom + c.Y);
        }
        public Rct Scaled(double s)
        {
            return new Rct(Left * s, Top * s, Right * s, Bottom * s);
        }
        public static Rct operator +(Rct r, Vec c) { return r.Translated(c); }
        public static Rct operator -(Rct r, Vec c) { return r.Translated(-c); }
        public static Rct operator *(Rct r, double s) { return r.Scaled(s); }
        /// <summary>
        /// The argument rectangle is contained by (inside) this rectangle.
        /// </summary>
        public bool Contains(Rct b)
        {
            return (!IsNull() && !b.IsNull() && b.Top >= Top && b.Left >= Left
                    && b.Bottom <= Bottom && b.Right <= Right);
        }
        /// <summary>
        /// Is the given point inside this rectangle?
        /// </summary>
        public bool Contains(Pt b)
        {
            return (!IsNull() && b.Y >= Top && b.X >= Left
                    && b.Y <= Bottom && b.X <= Right);
        }
        public override bool Equals(object obj)
        {
            return obj is Rct && this == (Rct)obj;
        }
        public override int GetHashCode()
        {
            return TopLeft.GetHashCode() ^ BottomRight.GetHashCode();
        }

        // both ways cause can't define operators on built-in types.
        public static implicit operator Rect(Rct r) { return r == Rct.Null ? Rect.Empty : new Rect(r.Left, r.Top, r.Width, r.Height); }
        public static implicit operator Rct(Rect r) { return r.IsEmpty ? Rct.Null : new Rct(new Pt(r.Left, r.Top), new Vec(r.Width, r.Height)); }

        public Rct Union(Rct b)
        {
            /* If one arg is null in only one direction, the other direction
               will influence the union, so must test */
            if (b.IsNull()) return this;
            if (IsNull()) return b;
            return new Rct(Math.Min(Left, b.Left), Math.Min(Top, b.Top),
                                  Math.Max(Right, b.Right), Math.Max(Bottom, b.Bottom));
        }
        public Rct Union(Pt b) { return Union(new Rct(b)); }
        public Rct Intersection(Rct b)
        {
            /* If one or the other is null, result will be null automatically */
            return new Rct(Math.Max(Left, b.Left), Math.Max(Top, b.Top),
                                 Math.Min(Right, b.Right), Math.Min(Bottom, b.Bottom));
        }
        public override string ToString()
        {
            return "Left=" + Left + " Top=" + Top + " Right=" + Right + " Bottom=" + Bottom;
        }
    }
}
