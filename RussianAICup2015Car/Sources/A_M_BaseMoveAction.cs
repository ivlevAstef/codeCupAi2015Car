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

    protected bool isEndAt(double ticks) {
      double sideDistance = game.TrackTileMargin + game.CarHeight * 0.75;

      if (car.Speed() < 5) {
        return false;
      }

      PointInt dirOut = path[0].DirOut;
      Vector dir = new Vector(dirOut.X, dirOut.Y);

      Vector wayEnd = GetWayEnd(path[0].Pos, dirOut);
      wayEnd = wayEnd + new Vector(dirOut.X * sideDistance, dirOut.Y * sideDistance);

      PhysicCar physicCar = new PhysicCar(car, game);
      physicCar.Iteration((int)ticks);

      return (physicCar.Pos - wayEnd).Dot(dir) > 0;
    }

    protected MoveEndType isEndAtAngle(double angleDt) {
      double sideDistance = game.TrackTileMargin + game.CarHeight * 0.75;
      double endSideDistance = game.TrackTileSize * 0.5 - (game.TrackTileMargin + game.CarHeight * 0.5);

      if (car.Speed() < 5) {
        return MoveEndType.NotArrival;
      }

      PointInt dirOut = path[0].DirOut;
      Vector dir = new Vector(dirOut.X, dirOut.Y).Normalize();

      Vector wayEnd = GetWayEnd(path[0].Pos, dirOut);
      Vector wayEndFinal = wayEnd + dir * sideDistance;

      PhysicCar physicCar = new PhysicCar(car, game);
      physicCar.setWheelTurn(Math.Sign(angleDt));

      double finalAngle = car.Angle + angleDt;
      int ticks = 0;
      for (ticks = 0; ticks < 50; ticks++) {
        physicCar.Iteration(1);
        if (Math.Abs(physicCar.Angle - finalAngle) <= Math.PI / 90) {
          return MoveEndType.NotArrival;
        }

        double needTicksToTurn = 100 * (finalAngle - physicCar.Angle);
        if (Math.Abs(physicCar.AngularSpeed) > 1.0e-7) {
          needTicksToTurn = (finalAngle - physicCar.Angle) / Math.Abs(physicCar.AngularSpeed);
        }
        double needTicksToZero = Math.Abs(physicCar.WheelTurn / game.CarWheelTurnChangePerTick);

        physicCar.setWheelTurn(needTicksToTurn * needTicksToZero);

        Vector distance = wayEnd - physicCar.Pos;
        if (distance.Dot(dir) < 0) {
          physicCar.setWheelTurn(0.0);
        }


        Vector distanceFinal = wayEndFinal - physicCar.Pos;

        if (Math.Abs(distanceFinal.Dot(dir.Perpendicular())) > endSideDistance) {
          return MoveEndType.SideCrash;
        }

        if (distanceFinal.Dot(dir) < 0) {
          return MoveEndType.Success;
        }

      }

      return MoveEndType.NotArrival;
    }
  }
}
