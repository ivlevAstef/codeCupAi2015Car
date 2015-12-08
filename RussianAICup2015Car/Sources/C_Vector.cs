using System;

namespace RussianAICup2015Car.Sources.Common {
  public class Vector : Point<double> {
    private static readonly double epsilon = 1.0e-9;

    public Vector(double all) : base(all) { }
    public Vector(double x, double y) : base(x, y) { }
    public Vector(Vector p) : base(p) { }

    public static Vector sincos(double angle) {
      return new Vector(Math.Cos(angle), Math.Sin(angle)).Normalize();
    }

    public static bool operator ==(Vector a, Vector b) {
      if (System.Object.ReferenceEquals(a, b)) {
        return true;
      }

      if (((object)a == null) || ((object)b == null)) {
        return false;
      }

      return Math.Abs(a.X - b.X) < epsilon && Math.Abs(a.Y - b.Y) < epsilon;
    }

    public static bool operator !=(Vector a, Vector b) {
      return !(a == b);
    }

    public override bool Equals(object obj) {
      return Equals(obj as Vector);
    }

    public bool Equals(Vector v) {
      return (null != v) && (this == v);
    }

    public override int GetHashCode() {
      return (X + 100 * Y).GetHashCode();
    }

    public double Length { get { return Math.Sqrt(X * X + Y * Y); } }

    public static Vector operator +(Vector v1, Vector v2) {
      return new Vector(v1.X + v2.X, v1.Y + v2.Y);
    }

    public static Vector operator -(Vector v1, Vector v2) {
      return new Vector(v1.X - v2.X, v1.Y - v2.Y);
    }

    public static Vector operator *(Vector v1, double c) {
      return new Vector(v1.X * c, v1.Y * c);
    }

    public static Vector operator /(Vector v1, double c) {
      return new Vector(v1.X / c, v1.Y / c);
    }

    public double Dot(Vector v) {
      return X * v.X + Y * v.Y;
    }

    public double Cross(Vector v) {
      return X * v.Y - Y * v.X;
    }

    public Vector Negative() {
      return new Vector(-X, -Y);
    }

    public Vector PerpendicularLeft() {
      return new Vector(Y, -X);
    }

    public Vector PerpendicularRight() {
      return new Vector(-Y, X);
    }

    public Vector Perpendicular() {
      return PerpendicularRight();
    }

    public Vector Normalize() {
      double len = Math.Sqrt(X * X + Y * Y);
      if (len < 1.0e-9) {
        return new Vector(X, Y);
      }
      return new Vector(X / len, Y / len);
    }

    public double Angle { get { return Math.Atan2(Y, X); } }

    public double GetAngleTo(Vector v, double angle = 0) {
      return GetAngleTo(v.X, v.Y, angle);
    }

    public double GetAngleTo(double x, double y, double angle = 0) {
      double absoluteAngle = Math.Atan2(y - this.Y, x - this.X);
      double relativeAngle = (absoluteAngle - angle);

      while (angle > Math.PI) {
        angle -= 2.0D * Math.PI;
      }

      while (angle < -Math.PI) {
        angle += 2.0D * Math.PI;
      }

      return angle;
    }

    public double GetDistanceTo(Vector v) {
      return GetDistanceTo(v.X, v.Y);
    }

    public double GetDistanceTo(double x, double y) {
      double xRange = x - X;
      double yRange = y - Y;
      return Math.Sqrt(xRange * xRange + yRange * yRange);
    }
  }
}
