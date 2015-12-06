using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System;

namespace RussianAICup2015Car.Sources.Common {
  public class TileDir : Point<int> {
    public static readonly TileDir Zero = new TileDir(0, 0);
    public static readonly TileDir Left = new TileDir(-1, 0);
    public static readonly TileDir Right = new TileDir(1, 0);
    public static readonly TileDir Up = new TileDir(0, -1);
    public static readonly TileDir Down = new TileDir(0, 1);

    public TileDir(int all) : base(all) { }
    public TileDir(int x, int y) : base(x, y) { }
    public TileDir(TileDir p) : base(p) { }

    public static TileDir TileDirByDirection(Direction directionType) {
      switch (directionType) {
      case Direction.Left:
        return Left;
      case Direction.Right:
        return Right;
      case Direction.Up:
        return Up;
      case Direction.Down:
        return Down;
      }
      return Zero; 
    }

    public static bool operator ==(TileDir a, TileDir b) {
      if (System.Object.ReferenceEquals(a, b)) {
        return true;
      }

      if (((object)a == null) || ((object)b == null)) {
        return false;
      }

      return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(TileDir a, TileDir b) {
      return !(a == b);
    }

    public override bool Equals(object obj) {
      return Equals(obj as TileDir);
    }

    public bool Equals(TileDir dir) {
      return (null != dir) && (this == dir);
    }

    public override int GetHashCode() {
      return (X ^ Y).GetHashCode();
    }

    public bool Correct() {
      return (0 == X && 1 == Math.Abs(Y)) || (0 == Y && 1 == Math.Abs(X));
    }

    public static TileDir operator +(TileDir d1, TileDir d2) {
      return new TileDir(d1.X + d2.X, d1.Y + d2.Y);
    }

    public static TileDir operator -(TileDir d1, TileDir d2) {
      return new TileDir(d1.X - d2.X, d1.Y - d2.Y);
    }

    public static TileDir operator *(TileDir d1, TileDir d2) {
      return new TileDir(d1.X * d2.X, d1.Y * d2.Y);
    }

    public static TileDir operator *(TileDir d1, int value) {
      return new TileDir(d1.X * value, d1.Y * value);
    }

    public TileDir Negative() {
      return new TileDir(-X, -Y);
    }

    public TileDir PerpendicularLeft() {
      return new TileDir(Y, -X);
    }

    public TileDir PerpendicularRight() {
      return new TileDir(-Y, X);
    }

    public TileDir Perpendicular() {
      return PerpendicularRight();
    }
  }
}
