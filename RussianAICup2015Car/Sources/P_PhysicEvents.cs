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
    private static double rotationFrictionFactor = 0;
    private static double wheelTurnChangePerTick = 0;

    public static void setupEnvironment(Game game) {
      rotationFrictionFactor = game.CarRotationFrictionFactor;
      wheelTurnChangePerTick = game.CarWheelTurnChangePerTick;
    }

    private double angle;

    public AngleReachEvent(double angle) {
      this.angle = angle;
    }

    public override PhysicEventType Type { get { return PhysicEventType.AngleReach; } }

    public override bool Check(PCar car) {
      double angleDeviation = angle.AngleDeviation(car.Angle);
      return Math.Abs(angleDeviation) < rotationFrictionFactor && Math.Abs(car.WheelTurn) < wheelTurnChangePerTick;
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
    private readonly List<ICollisionObject> collisionObjects;

    //TODO: need set collision object
    public MapCrashEvent(List<ICollisionObject> additionalCollisionObjects) {
      this.collisionObjects = additionalCollisionObjects;
    }

    public override PhysicEventType Type { get { return PhysicEventType.MapCrash; } }

    public override bool Check(PCar car) {
      CollisionRect carRect = new CollisionRect(car);

      List<CollisionInfo> collisions = new List<CollisionInfo>();
      if (null != collisionObjects) {
        collisions.AddRange(CollisionDetector.CheckCollision(carRect, collisionObjects));
      }

      collisions.AddRange(CollisionDetector.CollisionsWithMap(carRect));

      if (!collisions.HasCollision()) {
        return false;
      }

      Vector normal = collisions.AverageNormalObj1();
      if (car.Speed.Dot(normal) > 0) {
        return false;
      }

      checkInfo = normal;
      return true;
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

  public class PassageTileEvent : PhysicEventBase {
    private TilePos tile;

    public PassageTileEvent(TilePos tile) {
      this.tile = tile;
    }

    public override PhysicEventType Type { get { return PhysicEventType.PassageTile; } }

    public override bool Check(PCar car) {
      TilePos carPos = new TilePos(car.Pos.X, car.Pos.Y);
      return carPos == tile;
    }
  }

  public class OutFromTileEvent : PhysicEventBase {
    private TilePos tile;

    public OutFromTileEvent(TilePos tile) {
      this.tile = tile;
    }

    public override PhysicEventType Type { get { return PhysicEventType.OutFromTile; } }

    public override bool Check(PCar car) {
      TilePos carPos = new TilePos(car.Pos.X, car.Pos.Y);
      return carPos != tile;
    }
  }
}
