using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  public abstract class MovingBase : BaseAction {
    public enum PathCheckResult {
      No,
      Unknown,
      Yes
    }

    protected Vector GetWayEnd(TilePos wayPos, TileDir dir, double mult = 1) {
      double distanceToSide = game.TrackTileSize * 0.5 - game.CarHeight * 0.5 - game.TrackTileMargin;

      double nextWaypointX = (wayPos.X + 0.5) * game.TrackTileSize + dir.X * mult * distanceToSide;
      double nextWaypointY = (wayPos.Y + 0.5) * game.TrackTileSize + dir.Y * mult * distanceToSide;
      return new Vector(nextWaypointX, nextWaypointY);
    }

    protected bool isStraight(int offset = 0) {
      int straightCount = 0;
      for (int i = offset; i < Math.Min(offset + 3, path.Count); i++) {
        if (path[i].DirIn.Equals(path[i].DirOut)) {
          straightCount++;
        } else {
          break;
        }
      }

      return straightCount >= 3;
    }

    protected PathCheckResult checkAround(int offset = 0) {
      if (2 + offset >= path.Count) {
        return PathCheckResult.Unknown;
      }

      TilePos posIn = path[1 + offset].Pos;
      TilePos posOut = path[2 + offset].Pos;

      TileDir dirIn = path[1 + offset].DirIn;
      TileDir dirOut = path[2 + offset].DirOut;

      if (null == dirOut || dirOut == TileDir.Zero) {
        return PathCheckResult.Unknown;
      }

      bool isLine = dirIn == path[1 + offset].DirOut || path[2 + offset].DirIn == dirOut;

      if (!isLine && dirIn == dirOut.Negative() && posIn != posOut) {
        return PathCheckResult.Yes;
      } else {
        return PathCheckResult.No;
      }
    }

    protected PathCheckResult checkTurn(int offset = 0) {
      if (1 + offset >= path.Count) {
        return PathCheckResult.Unknown;
      }

      TileDir dirIn = path[offset + 1].DirIn;
      TileDir dirOut = path[offset + 1].DirOut;

      if (null == dirOut || dirOut == TileDir.Zero) {
        return PathCheckResult.Unknown;
      }

      if (dirIn == dirOut.PerpendicularLeft() || dirIn == dirOut.PerpendicularRight()) {
        return PathCheckResult.Yes;
      } else {
        return PathCheckResult.No;
      }
    }

    protected PathCheckResult checkSnakeWithOffset(int offset = 0) {
      if (3 + offset >= path.Count) {
        return PathCheckResult.Unknown;
      }

      TilePos posIn = path[1 + offset].Pos;
      TilePos posOut = path[2 + offset].Pos;

      TileDir dirIn = path[1 + offset].DirIn;
      TileDir dirOut = path[2 + offset].DirOut;

      if (null == dirOut || dirOut == TileDir.Zero) {
        return PathCheckResult.Unknown;
      }

      TileDir dir = new TileDir(posOut.X - posIn.X, posOut.Y - posIn.Y);

      if (dirIn == dirOut && (dir == dirIn.PerpendicularLeft() || dir == dirIn.PerpendicularRight())) {
        return PathCheckResult.Yes;
      } else {
        return PathCheckResult.No;
      }
    }

  }
}
