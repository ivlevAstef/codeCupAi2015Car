using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianAICup2015Car.Sources {
  class Point<Type> where Type : struct {
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

    public bool Equals(Point<Type> p) {
      if ((object)p == null) {
        return false;
      }

      return ((dynamic)X == (dynamic)p.X) && ((dynamic)Y == (dynamic)p.Y);
    }

    public Point<Type> Add(Point<Type> p) {
      return new Point<Type>((dynamic)X + (dynamic)p.X, (dynamic)Y + (dynamic)p.Y);
    }
  }
}
