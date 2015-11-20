using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_PreTurnAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.WayCells.Length, "incorrect way cells count.");

      if (!path.ShortWayCells[0].DirIn.Equals(path.ShortWayCells[0].DirOut)) {
        return false;
      }

      PointInt dirIn = path.ShortWayCells[1].DirIn;
      PointInt dirOut = path.ShortWayCells[1].DirOut;

      if (null == dirOut) {
        return true;
      }

      return dirIn.Equals(dirOut.Perpendicular()) || dirIn.Equals(dirOut.Perpendicular().Negative()); 
    }

    public override void execute(Move move) {
      double maxSpeed = game.TrackTileSize / 28;//around 28
      PointInt dirMove = path.FirstWayCell.DirOut;

      if (car.Speed() > maxSpeed) {
        move.IsBrake = true;
      }

      PointDouble wayEnd = GetWayEnd(path.FirstWayCell.Pos, dirMove);
      double distanceToEnd = car.GetDistanceTo(wayEnd, dirMove);
      double ticksToEnd = distanceToEnd / car.SpeedN(dirMove);

      double stallSpeed = ticksToEnd * game.CarLengthwiseMovementFrictionFactor;
      double exceesSpeed = Math.Max(0.0, (car.Speed() - stallSpeed) - maxSpeed);

      move.EnginePower = (1.0 - exceesSpeed * exceesSpeed);

      double magnitedAngleNegative = magniteToSide(dirMove, GetDirOut().Negative());
      double magnitedAngle = car.GetAngleTo(car.X + dirMove.X, car.Y + dirMove.Y);

      double procentToEnd = distanceToEnd / game.TrackTileSize;
      double finalAngle = magnitedAngleNegative * procentToEnd + magnitedAngle * (1.0 - procentToEnd);

      move.WheelTurn = 0.5 * finalAngle * car.WheelTurnFactor(game);
    }

    public override HashSet<ActionType> blockers { get { return new HashSet<ActionType>() { ActionType.InitialFreeze, ActionType.StuckOut }; } }

    private PointDouble GetWayEnd(PointInt wayPos, PointInt dir) {
      double nextWaypointX = (wayPos.X + 0.5 + dir.X * 0.5) * game.TrackTileSize;
      double nextWaypointY = (wayPos.Y + 0.5 + dir.Y * 0.5) * game.TrackTileSize;
      return new PointDouble(nextWaypointX, nextWaypointY);
    }

    private double magniteToSide(PointInt dir, PointInt normal) {
      double powerTilt = game.TrackTileSize * 1.0;
      double sideDistance = (game.TrackTileSize * 0.5) - game.TrackTileMargin - game.CarWidth * 0.5;

      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5) * game.TrackTileSize;

      double sideX = centerX + normal.X * sideDistance;
      double sideY = centerY + normal.Y * sideDistance;

      return car.GetAngleTo(new PointDouble(sideX, sideY), dir, powerTilt);
    }

    private PointInt GetDirOut() {
      PointInt dirOut = path.ShortWayCells[1].DirOut;

      if (null == dirOut) {
        PointInt dirIn = path.ShortWayCells[1].DirIn;
        dirOut = new PointInt(0);
        foreach (PointInt dir in path.ShortWayCells[1].Dirs) {
          if (!dir.Equals(dirIn) && !dir.Equals(dirIn.Negative())) {
            dirOut = dirOut.Add(dir);
          }
        }
        //for crossroad return zero point.
      }

      return dirOut;
    }
  }
}
