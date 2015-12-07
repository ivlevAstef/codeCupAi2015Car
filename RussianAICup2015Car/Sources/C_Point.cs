using System;

namespace RussianAICup2015Car.Sources.Common {
  public class Point<Type> where Type : struct {
    public Type X { get { return x; } }
    public Type Y { get { return y; } }

    private Type x;
    private Type y;

    public Point(Type all) {
      set(all, all);
    }

    public Point(Type x, Type y) {
      set(x, y);
    }

    public Point(Point<Type> p) {
      set(p.X, p.Y);
    }

    public void set(Type x, Type y) {
      this.x = x;
      this.y = y;
    }
  }
}
