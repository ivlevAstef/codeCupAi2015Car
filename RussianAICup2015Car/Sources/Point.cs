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

    public PointInt Perpendicular() {
      return new PointInt(Y, -X);
    }
  }


  public class PointDouble : Point<double> {
    public PointDouble(double all) : base(all) {}
    public PointDouble(double x, double y) : base(x, y) {}
    public PointDouble(PointDouble p) : base(p) {}

    public override bool Equals(object obj) {
      var p = obj as PointDouble;
      if (null == p) {
        return false;
      }

      return (X == p.X) && (Y == p.Y);
    }

    public override int GetHashCode() {
      return (X + 100 * Y).GetHashCode();
    }

    public PointDouble Add(PointDouble p) {
      return new PointDouble(X + p.X, Y + p.Y);
    }
  }
}
