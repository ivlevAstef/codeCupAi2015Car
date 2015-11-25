using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_PreTurnAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      if (!path[1].DirIn.Equals(path[1].DirOut)) {
        return false;
      }

      PointInt dirIn = path[2].DirIn;
      PointInt dirOut = path[2].DirOut;

      if (null == dirOut) {
        return true;
      }

      return dirIn.Equals(dirOut.Perpendicular()) || dirIn.Equals(dirOut.Perpendicular().Negative()); 
    }

    public override void execute(Move move) {
      PointInt dirMove = path[0].DirOut;

      if (car.Speed() > 33 - car.EnginePower * 5  - car.RemainingNitroTicks * 0.03) {
        move.IsBrake = true;
      }

      move.EnginePower = 1.0;

      PointDouble wayEnd = GetWayEnd(path[0].Pos, dirMove);
      double distanceToEnd = car.GetDistanceTo(wayEnd, dirMove);
      double ticksToEnd = distanceToEnd / car.SpeedN(dirMove);

      double magnitedAngleNegative = magniteToSide(dirMove, GetDirOut().Negative());
      double magnitedAngle = magniteToSide(dirMove, new PointInt(0));

      double procentToEnd = distanceToEnd / game.TrackTileSize;
      double finalAngle = (magnitedAngleNegative - magnitedAngle) * 0.25 * procentToEnd + magnitedAngle;

      move.WheelTurn = car.WheelTurnForAngle(finalAngle, game);
    }

    public override HashSet<ActionType> GetParallelsActions() {
      return new HashSet<ActionType>() {
        ActionType.Shooting
      };
    }

    private PointDouble GetWayEnd(PointInt wayPos, PointInt dir) {
      double nextWaypointX = (wayPos.X + 0.5 + dir.X * 0.5) * game.TrackTileSize;
      double nextWaypointY = (wayPos.Y + 0.5 + dir.Y * 0.5) * game.TrackTileSize;
      return new PointDouble(nextWaypointX, nextWaypointY);
    }

    private double magniteToSide(PointInt dir, PointInt normal) {
      double powerTilt = game.TrackTileSize * 0.75;
      double sideDistance = (game.TrackTileSize * 0.5) - game.TrackTileMargin - game.CarWidth * 0.5;

      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5) * game.TrackTileSize;

      double sideX = centerX + normal.X * sideDistance;
      double sideY = centerY + normal.Y * sideDistance;

      return car.GetAngleTo(new PointDouble(sideX, sideY), dir, powerTilt);
    }

    private PointInt GetDirOut() {
      PointInt dirOut = path[2].DirOut;

      if (null == dirOut) {
        PointInt dirIn = path[2].DirIn;
        dirOut = new PointInt(0);
        foreach (PointInt dir in path[2].DirOuts) {
          if (!dir.Equals(dirIn) && !dir.Equals(dirIn.Negative())) {
            dirOut = dirOut + dir;
          }
        }
        //for crossroad return zero point.
      }

      return dirOut;
    }
  }
}
