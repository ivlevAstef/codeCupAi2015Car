using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_TurnAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.WayCells.Length, "incorrect way cells count.");

      if (!path.WayCells[0].DirIn.Equals(path.WayCells[0].DirOut)) {
        return false;
      }

      if (null != path.WayCells[2].DirOut && !path.WayCells[2].DirIn.Equals(path.WayCells[2].DirOut)) {
        return false;
      }

      PointInt dirIn = path.WayCells[1].DirIn;
      PointInt dirOut = path.WayCells[1].DirOut;

      return dirIn.Equals(dirOut.Perpendicular()) || dirIn.Equals(dirOut.Perpendicular().Negative()); 
    }

    public override void execute(Move move) {
      PointInt dirMove = path.FirstWayCell.DirOut;
      PointInt dirEnd = path.ShortWayCells[0].DirOut;

      PointDouble endPoint = GetWaySideEnd(path.FirstWayCell.Pos, dirMove, dirEnd);

      double needAngle = car.GetAngleTo(endPoint.X, endPoint.Y);
      double needWheelTurn = 25 * needAngle / (Math.PI * 0.25);
      needWheelTurn = Math.Max(-1.0, Math.Min(1.0, needWheelTurn));

      move.WheelTurn = needWheelTurn;

      move.EnginePower = 1.0;

      double normalAngle = car.GetAngleTo(car.X + dirMove.X + dirEnd.X, car.Y + dirMove.Y + dirEnd.Y);
      double procentToEnd = car.GetDistanceTo(endPoint, dirMove) / game.TrackTileSize;

      double diffAngle = (normalAngle - needAngle);
      if (diffAngle > 0 && procentToEnd < 0.75 && car.SpeedN(dirMove) * diffAngle * diffAngle > 4) {
        move.IsBrake = true;
      }

      if (car.SpeedN(dirMove) > 23) {
        move.IsBrake = true;
      }
    }

    public override HashSet<ActionType> blockers { get { return new HashSet<ActionType>() { ActionType.InitialFreeze, ActionType.StuckOut }; } }

    private PointDouble GetWaySideEnd(PointInt pos, PointInt dir, PointInt normal) {
      double endSideDistance = game.TrackTileSize * 0.5 - game.TrackTileMargin - game.CarHeight * 0.5;

      PointDouble wayEnd = GetWayEnd(pos, dir);

      double endX = wayEnd.X + normal.X * endSideDistance;
      double endY = wayEnd.Y + normal.Y * endSideDistance;

      return new PointDouble(endX, endY);
    }

    private PointDouble GetWayEnd(PointInt wayPos, PointInt dir) {
      double nextWaypointX = (wayPos.X + 0.5 + dir.X * 0.5) * game.TrackTileSize;
      double nextWaypointY = (wayPos.Y + 0.5 + dir.Y * 0.5) * game.TrackTileSize;
      return new PointDouble(nextWaypointX, nextWaypointY);
    }
  }
}
