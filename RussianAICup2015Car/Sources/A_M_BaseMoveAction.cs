﻿using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  public abstract class A_M_BaseMoveAction : A_BaseAction {

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

    protected bool isEndAtAngle(double angleDt) {
      double sideDistance = game.TrackTileMargin + game.CarHeight * 1.0;
      double endSideDistance = game.TrackTileSize * 0.5 - (game.TrackTileMargin + game.CarHeight * 0.5);

      if (car.Speed() < 5) {
        return false;
      }

      PointInt dirOut = path[0].DirOut;
      Vector dir = new Vector(dirOut.X, dirOut.Y).Normalize();

      Vector wayEnd = GetWayEnd(path[0].Pos, dirOut);
      wayEnd = wayEnd + dir * sideDistance;

      PhysicCar physicCar = new PhysicCar(car, game);
      physicCar.setWheelTurn(Math.Sign(angleDt));

      double finalAngle = car.Angle + angleDt;
      int ticks = 0;
      for (ticks = 0; ticks < 50; ticks++) {
        physicCar.Iteration(1);
        if (Math.Abs(physicCar.Angle - finalAngle) <= Math.PI / 90) {
          return false;
        }

        Vector distance = physicCar.Pos - wayEnd;

        if (Math.Abs(distance.Dot(dir.Perpendicular())) > endSideDistance) {
          return false;
        }

        if (distance.Dot(dir) > 0) {
          return true;
        }

      }

      return false;
    }
  }
}
