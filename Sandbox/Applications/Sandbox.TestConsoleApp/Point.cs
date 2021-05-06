using System;

namespace Sandbox.TestConsoleApp
{
    internal readonly struct Point : IEquatable<Point>, IComparable<Point>, IComparable
    {
        public static int Max;

        static Point()
        {
            Max = 42;
        }

        public int X { get; }
        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string? ToString() => $"({X.ToString()}, {Y.ToString()})";
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override bool Equals(object? obj) => obj is Point point && Equals(point);
        public bool Equals(Point other) => X == other.X && Y == other.Y;

        public static bool operator ==(Point left, Point right) => left.Equals(right);
        public static bool operator !=(Point left, Point right) => !left.Equals(right);

        public int CompareTo(Point other)
        {
            var originalValue = Math.Sqrt(X * X + Y * Y);
            var otherValue = Math.Sqrt(other.X * other.X + other.Y * other.Y);
            return Math.Sign(originalValue - otherValue);
        }

        public int CompareTo(object? obj)
        {
            if (obj is not Point point) throw new ArgumentException($"Object is not {nameof(Point)}.", nameof(obj));
            return CompareTo(point);
        }

        public static bool operator <(Point left, Point right) => left.CompareTo(right) < 0;
        public static bool operator <=(Point left, Point right) => left.CompareTo(right) <= 0;
        public static bool operator >(Point left, Point right) => left.CompareTo(right) > 0;
        public static bool operator >=(Point left, Point right) => left.CompareTo(right) >= 0;
    }
}
