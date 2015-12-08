using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  public abstract class MovingBase : BaseAction {
    public enum MoveEndType {
      Success,
      NotArrival,
      SideCrash
    }

    protected Vector GetWayEnd(TilePos wayPos, TileDir dir, double mult = 0.5) {
      double nextWaypointX = (wayPos.X + 0.5 + dir.X * mult) * game.TrackTileSize;
      double nextWaypointY = (wayPos.Y + 0.5 + dir.Y * mult) * game.TrackTileSize;
      return new Vector(nextWaypointX, nextWaypointY);
    }

  }
}
