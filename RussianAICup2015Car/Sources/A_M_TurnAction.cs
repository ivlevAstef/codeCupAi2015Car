﻿using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_TurnAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.WayCells.Length, "incorrect way cells count.");

      PointInt dirIn = path.WayCells[1].DirIn;
      PointInt dirOut = path.WayCells[1].DirOut;

      return dirIn.Equals(dirOut.Perpendicular()) || dirIn.Equals(dirOut.Perpendicular().Negative()); 
    }

    public override void execute(Move move) {
      PointInt dirMove = path.FirstWayCell.DirOut;
      PointInt dirEnd = path.ShortWayCells[0].DirOut;

      PointDouble wayEnd = GetWayEnd(path.FirstWayCell.Pos, dirMove);
      PointDouble endPoint = GetWaySideEnd(path.FirstWayCell.Pos, dirMove, dirEnd);

      double sign = GetSign(dirMove, dirEnd);

      double needAngle = car.GetAngleTo(endPoint.X, endPoint.Y);

      double normalAngle = car.GetAngleTo(car.X + dirMove.X + dirEnd.X, car.Y + dirMove.Y + dirEnd.Y);
      double distanceToEnd = car.GetDistanceTo(wayEnd, dirMove);
      double procentToEnd = distanceToEnd / game.TrackTileSize;

      double angle = needAngle;
      if (distanceToEnd < game.CarWidth * 0.5 && sign * normalAngle > 0) {
        angle = car.GetAngleTo(car.X + dirEnd.X, car.Y + dirEnd.Y);
      }

      move.WheelTurn = car.WheelTurnForAngle(angle, game);

      double diffAngle = sign * (normalAngle - angle);
      if (diffAngle > 0 && procentToEnd < 0.5 && car.SpeedN(dirMove) * diffAngle > 3.5) {
        move.IsBrake = true;
      }

      if (car.SpeedN(dirMove) > 21) {
        move.IsBrake = true;
      } else {
        move.EnginePower = 1.0;
      }
    }

    public override HashSet<ActionType> GetBlocks() {
      return new HashSet<ActionType>() { 
        ActionType.InitialFreeze,
        ActionType.Forward,
        ActionType.Backward,
        ActionType.PreTurn,
        ActionType.MoveToBonus
      }; 
    }

    public override HashSet<ActionType> GetDynamicBlocks() {
      return new HashSet<ActionType>() { 
        ActionType.UseNitro,
      }; 
    }

    private double GetSign(PointInt dir1, PointInt dir2) {
      double changedSign = Math.Abs(dir1.X + dir1.Y + dir2.X + dir2.Y) - 1;
      if (0 == dir2.X) {
        return changedSign;
      }
      return -changedSign;
    }

    private double AngleToWheelTurn(double angle) {
      double scalar = car.SpeedX * Math.Sin(car.Angle) + car.SpeedY * Math.Cos(car.Angle);

      return angle / (game.CarAngularSpeedFactor * Math.Abs(scalar));
    }

    private PointDouble GetWaySideEnd(PointInt pos, PointInt dir, PointInt normal) {
      double sideDistance = game.TrackTileMargin + game.CarHeight * 0.5;
      double endSideDistance = game.TrackTileSize * 0.5 - sideDistance;

      PointDouble wayEnd = GetWayEnd(pos, dir);

      double endX = wayEnd.X + normal.X * endSideDistance + dir.X * sideDistance;
      double endY = wayEnd.Y + normal.Y * endSideDistance + dir.Y * sideDistance;

      return new PointDouble(endX, endY);
    }

    private PointDouble GetWayEnd(PointInt wayPos, PointInt dir) {
      double nextWaypointX = (wayPos.X + 0.5 + dir.X * 0.5) * game.TrackTileSize;
      double nextWaypointY = (wayPos.Y + 0.5 + dir.Y * 0.5) * game.TrackTileSize;
      return new PointDouble(nextWaypointX, nextWaypointY);
    }
  }
}
