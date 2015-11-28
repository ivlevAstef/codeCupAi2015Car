using System;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {
  public class Point<Type> where Type : struct {
    public Type X;
    public Type Y;

    public Point(Type all) {
      X = all;
      Y = all;
    }

    public Point(Type x, Type y) {
      X = x;
      Y = y;
    }

    public Point(Point<Type> p) {
      X = p.X;
      Y = p.Y;
    }
  }

  public class PointInt : Point<int> {
    public PointInt(int all) : base(all) {}
    public PointInt(int x, int y) : base(x,y) {}
    public PointInt(PointInt p) : base(p){}

    public override bool Equals(object obj) {
      var p = obj as PointInt;
      if (null == p) {
        return false;
      }

      return (X == p.X) && (Y == p.Y);
    }

    public override int GetHashCode() {
      return (X^Y).GetHashCode();
    }

    public static PointInt operator+(PointInt p1, PointInt p2) {
      return new PointInt(p1.X + p2.X, p1.Y + p2.Y);
    }

    public static PointInt operator-(PointInt p1, PointInt p2) {
      return new PointInt(p1.X - p2.X, p1.Y - p2.Y);
    }

    public PointInt Negative() {
      return new PointInt(-X, -Y);
    }

    public PointInt PerpendicularLeft() {
      return new PointInt(Y, -X);
    }

    public PointInt PerpendicularRight() {
      return new PointInt(-Y, X);
    }

    public PointInt Perpendicular() {
      return PerpendicularRight();
    }
  }


  public class Vector : Point<double> {
    public Vector(double all) : base(all) {}
    public Vector(double x, double y) : base(x, y) {}
    public Vector(Vector p) : base(p) {}

    public static Vector sincos(double angle) {
      return new Vector(Math.Cos(angle), Math.Sin(angle)).Normalize();
    }

    public override bool Equals(object obj) {
      var p = obj as Vector;
      if (null == p) {
        return false;
      }

      return (X == p.X) && (Y == p.Y);
    }

    public override int GetHashCode() {
      return (X + 100 * Y).GetHashCode();
    }

    public double Length { get { return Math.Sqrt(X * X + Y * Y); } }

    public static Vector operator +(Vector p1, Vector p2) {
      return new Vector(p1.X + p2.X, p1.Y + p2.Y);
    }

    public static Vector operator -(Vector p1, Vector p2) {
      return new Vector(p1.X - p2.X, p1.Y - p2.Y);
    }

    public static Vector operator *(Vector p1, double c) {
      return new Vector(p1.X *c, p1.Y *c);
    }

    public static Vector operator /(Vector p1, double c) {
      return new Vector(p1.X / c, p1.Y / c);
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
      return new Vector(X / len, Y / len);
    }

    public double GetAngleTo(Vector v, double angle = 0) {
      return GetAngleTo(v.X, v.Y, angle);
    }

    public double GetAngleTo(double x, double y, double angle = 0) {
      double absoluteAngle = Math.Atan2(y - this.Y, x - this.X);
      double relativeAngle = absoluteAngle - angle;

      while (relativeAngle > Math.PI) {
        relativeAngle -= 2.0D * Math.PI;
      }

      while (relativeAngle < -Math.PI) {
        relativeAngle += 2.0D * Math.PI;
      }

      return relativeAngle;
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
