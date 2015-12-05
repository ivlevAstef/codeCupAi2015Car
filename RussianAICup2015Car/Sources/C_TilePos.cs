using System;

namespace RussianAICup2015Car.Sources.Common {
  public class TilePos : Point<int> {
    public static double TileSize;

    public TilePos(int all) : base(all) { }
    public TilePos(int x, int y) : base(x, y) { }
    public TilePos(TilePos p) : base(p) { }
    public TilePos(double x, double y)
      : base(0) {
      X = (int)(x / TileSize);
      Y = (int)(y / TileSize);
    }

    public static bool operator ==(TilePos a, TilePos b) {
      if (System.Object.ReferenceEquals(a, b)) {
        return true;
      }

      if (((object)a == null) || ((object)b == null)) {
        return false;
      }

      return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(TilePos a, TilePos b) {
      return !(a == b);
    }

    public override bool Equals(object obj) {
      return Equals(obj as TilePos);
    }

    public bool Equals(TilePos p) {
      return (null != p) && (this == p);
    }

    public override int GetHashCode() {
      return (X ^ Y).GetHashCode();
    }

    public static TilePos operator +(TilePos p, TileDir d) {
      return new TilePos(p.X + d.X, p.Y + d.Y);
    }

    public static TilePos operator -(TilePos p, TileDir d) {
      return new TilePos(p.X - d.X, p.Y - d.Y);
    }

    public static TileDir operator -(TilePos p1, TilePos p2) {
      return new TileDir(p1.X - p2.X, p1.Y - p2.Y);
    }

  }
}
