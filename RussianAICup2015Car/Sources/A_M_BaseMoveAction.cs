using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  public abstract class A_M_BaseMoveAction : A_BaseAction {
    public enum MoveEndType {
      Success,
      NotArrival,
      SideCrash
    }

    protected Vector GetWaySideEnd(PointInt pos, PointInt dir, PointInt normal) {
      double sideDistance = game.TrackTileMargin + game.CarHeight * 0.5;
      double endSideDistance = game.TrackTileSize * 0.5 - sideDistance;

      Vector wayEnd = GetWayEnd(pos, dir);

      double endX = wayEnd.X + normal.X * endSideDistance + dir.X * sideDistance;
      double endY = wayEnd.Y + normal.Y * endSideDistance + dir.Y * sideDistance;

      return new Vector(endX, endY);
    }

    protected Vector GetWayEnd(PointInt wayPos, PointInt dir) {
      double nextWaypointX = (wayPos.X + 0.5 + dir.X * 0.5) * game.TrackTileSize;
      double nextWaypointY = (wayPos.Y + 0.5 + dir.Y * 0.5) * game.TrackTileSize;
      return new Vector(nextWaypointX, nextWaypointY);
    }

  }
}
