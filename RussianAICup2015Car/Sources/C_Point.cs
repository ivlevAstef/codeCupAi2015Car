using System;

namespace RussianAICup2015Car.Sources.Common {
  public class Point<Type> where Type : struct {
    public readonly Type X;
    public readonly Type Y;

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
}
