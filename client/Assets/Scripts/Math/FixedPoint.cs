using System;

namespace MOBA.Math
{
    public struct FixedPoint : IEquatable<FixedPoint>, IComparable<FixedPoint>
    {
        public const int SCALE = 1000;
        public int RawValue { get; }

        public FixedPoint(int rawValue)
        {
            RawValue = rawValue;
        }

        public static FixedPoint FromInt(int value) => new FixedPoint(value * SCALE);
        public static FixedPoint FromRaw(int raw) => new FixedPoint(raw);

        public float ToFloat() => (float)RawValue / SCALE;

        // Operators
        public static FixedPoint operator +(FixedPoint a, FixedPoint b) => new FixedPoint(a.RawValue + b.RawValue);
        public static FixedPoint operator -(FixedPoint a, FixedPoint b) => new FixedPoint(a.RawValue - b.RawValue);
        public static FixedPoint operator -(FixedPoint a) => new FixedPoint(-a.RawValue);
        public static FixedPoint operator *(FixedPoint a, FixedPoint b) => new FixedPoint((int)(((long)a.RawValue * b.RawValue) / SCALE));
        public static FixedPoint operator /(FixedPoint a, FixedPoint b) => new FixedPoint((int)(((long)a.RawValue * SCALE) / b.RawValue));

        public static bool operator ==(FixedPoint a, FixedPoint b) => a.RawValue == b.RawValue;
        public static bool operator !=(FixedPoint a, FixedPoint b) => a.RawValue != b.RawValue;
        public static bool operator <(FixedPoint a, FixedPoint b) => a.RawValue < b.RawValue;
        public static bool operator >(FixedPoint a, FixedPoint b) => a.RawValue > b.RawValue;
        public static bool operator <=(FixedPoint a, FixedPoint b) => a.RawValue <= b.RawValue;
        public static bool operator >=(FixedPoint a, FixedPoint b) => a.RawValue >= b.RawValue;

        // Math
        public static FixedPoint Abs(FixedPoint v) => new FixedPoint(v.RawValue < 0 ? -v.RawValue : v.RawValue);
        public static FixedPoint Min(FixedPoint a, FixedPoint b) => a < b ? a : b;
        public static FixedPoint Max(FixedPoint a, FixedPoint b) => a > b ? a : b;

        public static FixedPoint Sqrt(FixedPoint v)
        {
            if (v.RawValue <= 0) return new FixedPoint(0);
            long n = (long)v.RawValue * SCALE;
            if (n <= 1) return new FixedPoint((int)n);
            long x = n;
            long y = (x + 1) / 2;
            while (y < x)
            {
                x = y;
                y = (x + n / x) / 2;
            }
            return new FixedPoint((int)x);
        }

        public static FixedPoint Distance2D(FixedPoint x1, FixedPoint y1, FixedPoint x2, FixedPoint y2)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            return Sqrt(dx * dx + dy * dy);
        }

        public bool Equals(FixedPoint other) => RawValue == other.RawValue;
        public override bool Equals(object obj) => obj is FixedPoint other && Equals(other);
        public override int GetHashCode() => RawValue.GetHashCode();
        public int CompareTo(FixedPoint other) => RawValue.CompareTo(other.RawValue);

        public override string ToString() => $"{ToFloat():F3}";
    }
}