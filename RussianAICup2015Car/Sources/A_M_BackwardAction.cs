using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_BackwardAction : A_BaseAction {
    private PointInt movedDir = null;

    public override bool valid() {
      Logger.instance.Assert(3 == path.WayCells.Length, "incorrect way cells count.");

      if (null != movedDir) {
        return true;
      }

      PointInt dir = path.FirstWayCell.DirOut;

      double angle = car.GetAngleTo(car.X + dir.X, car.Y + dir.Y);

      return Math.Abs(angle) > (2 * Math.PI / 3) && car.Speed() < 5;///120 degrees
    }

    public override void execute(Move move) {
      move.EnginePower = -1.0;

      if (null == movedDir) {
        movedDir = path.FirstWayCell.DirOut;
      }

      PointInt currentDir = path.FirstWayCell.DirOut;

      double magnitedAngle = 0;
      if (movedSign(movedDir) < 0) {
        move.IsBrake = true;

        magnitedAngle = magniteToCenter(movedDir);
      } else {
        PointDouble wayEnd = GetWayEnd(path.FirstWayCell.Pos, movedDir);
        double procentToEnd = car.GetDistanceTo(wayEnd.X, wayEnd.Y) / game.TrackTileSize;
        bool perpendicular = currentDir.Equals(movedDir.Perpendicular()) || currentDir.Equals(movedDir.Perpendicular().Negative());

        if (procentToEnd < 0.5 && perpendicular) {
          magnitedAngle = magniteToEnd(movedDir, currentDir.Negative());
          move.EnginePower = -Math.Max(0.1, procentToEnd);
        } else {
          magnitedAngle = 0.25 * magniteToCenter(movedDir);
        }

        if (procentToEnd < 0.05 && perpendicular) {
          movedDir = null;
          move.IsBrake = true;
          move.EnginePower = 0;
        }
      }

      double magnitedForce = car.WheelTurnForAngle(magnitedAngle, game);

      Logger.instance.Debug("Angle {0} wheelTurn {1}", magnitedAngle, magnitedForce);

      move.WheelTurn = magnitedForce;
    }

    public override HashSet<ActionType> GetParallelsActions() {
      return new HashSet<ActionType>() {
        ActionType.Shooting
      };
    } 

    private double magniteToCenter(PointInt dir) {
      double powerTilt = movedSign(dir) * game.TrackTileSize * 1;

      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5) * game.TrackTileSize;

      return car.GetAngleTo(new PointDouble(centerX, centerY), dir, powerTilt);
    }

    private double magniteToEnd(PointInt dir, PointInt normal) {
      PointInt pos = new PointInt((int)(car.X / game.TrackTileSize), (int)(car.Y / game.TrackTileSize));
      PointDouble endPoint = GetWaySideEnd(pos, dir, normal);

      return car.GetAngleTo(endPoint.X, endPoint.Y);
    }

    private double movedSign(PointInt dir) {
      double speed = car.SpeedX * dir.X + car.SpeedY * dir.Y;
      if (Math.Abs(speed) < 1.0e-3) {
        return 0;
      }

      return Math.Sign(speed);
    }

    private PointDouble GetWayEnd(PointInt wayPos, PointInt dir) {
      double nextWaypointX = (wayPos.X + 0.5 + dir.X * 0.5) * game.TrackTileSize;
      double nextWaypointY = (wayPos.Y + 0.5 + dir.Y * 0.5) * game.TrackTileSize;
      return new PointDouble(nextWaypointX, nextWaypointY);
    }

    private PointDouble GetWaySideEnd(PointInt pos, PointInt dir, PointInt normal) {
      double sideDistance = game.TrackTileMargin + game.CarHeight * 0.5;
      double endSideDistance = game.TrackTileSize * 0.5 - sideDistance;

      PointDouble wayEnd = GetWayEnd(pos, dir);

      double endX = wayEnd.X + normal.X * endSideDistance + dir.X * sideDistance;
      double endY = wayEnd.Y + normal.Y * endSideDistance + dir.Y * sideDistance;

      return new PointDouble(endX, endY);
    }
  }
}
