using System;

namespace RussianAICup2015Car.Sources.Common {
  public class TilePos {
    public static double TileSize;

    public int X;
    public int Y;

    public TilePos(int all) { X = all; Y = all; }
    public TilePos(int x, int y) { this.X = x; this.Y = y; }
    public TilePos(TilePos p) { X = p.X; Y = p.Y; }
    public TilePos(double x, double y) {
      X = (int)(x / TileSize);
      Y = (int)(y / TileSize);
    }

    public Vector ToVector(double anchorX, double anchorY) {
      return new Vector(X + anchorX, Y + anchorY) * TileSize;
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

    public TilePos Projection(TileDir axis) {
      return new TilePos(X * Math.Abs(axis.X), Y * Math.Abs(axis.Y));
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
