using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public class PassageLineEvent : PhysicEventBase {
    private Vector dir;
    private Vector pos;

    public PassageLineEvent(TileDir dirMove, Vector pos) {
      this.dir = new Vector(dirMove.X, dirMove.Y);
      this.pos = pos;
    }

    public override PhysicEventType Type { get { return PhysicEventType.PassageLine; } }

    public override bool Check(PCar car) {
      return Math.Sign((car.Pos - pos).Dot(dir)) != Math.Sign((car.LastPos - pos).Dot(dir));
    }
  }

  public class AngleReachEvent : PhysicEventBase {
    public static double RotationFrictionFactor = 0;
    public static double WheelTurnChangePerTick = 0;

    private double angle;

    public AngleReachEvent(double angle) {
      this.angle = angle;
    }

    public override PhysicEventType Type { get { return PhysicEventType.AngleReach; } }

    public override bool Check(PCar car) {
      double angleDeviation = angle.AngleDeviation(car.Angle);
      return Math.Abs(angleDeviation) < RotationFrictionFactor && Math.Abs(car.WheelTurn) < WheelTurnChangePerTick;
    }
  }

  public class SpeedReachEvent : PhysicEventBase {
    private double accuracy;

    public SpeedReachEvent(double accuracy = 1.0e-3) {
      this.accuracy = accuracy;
    }

    public override PhysicEventType Type { get { return PhysicEventType.SpeedReach; } }

    public override bool Check(PCar car) {
      return (car.Speed.Normalize() - car.Dir).Length < accuracy;
    }
  }

  public class MapCrashEvent : PhysicEventBase {
    private Vector idealPos;
    private TileDir dirMove;

    //TODO: need set collision object
    public MapCrashEvent(Vector idealPos, TileDir dirMove) {
      this.idealPos = idealPos;
      this.dirMove = dirMove;
    }

    public override PhysicEventType Type { get { return PhysicEventType.MapCrash; } }

    public override bool Check(PCar car) {
      TilePos carPos = new TilePos(car.Pos.X, car.Pos.Y);
      TilePos endPos = new TilePos(idealPos.X, idealPos.Y);
      bool isEndPos = carPos.Projection(dirMove) == endPos.Projection(dirMove);

      TileDir[] additionalDirs = isEndPos ? null : new TileDir[] { dirMove.PerpendicularLeft(), dirMove.PerpendicularRight() };

      Vector normal = CollisionDetector.instance.IntersectCarWithMap(car.Pos, car.Dir, additionalDirs);
      if (null != normal && car.Speed.Dot(normal) < 0) {
        checkInfo = normal;
        return true;
      }
      return false;
    }
  }

  public class IntersectOilStickEvent : PhysicEventBase {
    private World world;

    public IntersectOilStickEvent(World world) {
      this.world = world;
    }

    public override PhysicEventType Type { get { return PhysicEventType.IntersectOilStick; } }

    public override bool Check(PCar car) {
      foreach (OilSlick stick in world.OilSlicks) {
        if ((car.Pos - new Vector(stick.X, stick.Y)).Length < stick.Radius) {
          car.traveledOnOil(stick);
          checkInfo = stick;
          return true;
        }
      }
      return false;
    }
  }
}
