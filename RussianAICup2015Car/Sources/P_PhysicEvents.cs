using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public class PassageLineEvent : PhysicEventBase {
    private Vector normal;
    private Vector pos;
    private double accuracity;

    public PassageLineEvent(Vector normalOut, Vector pos, double accuracity) {
      this.normal = normalOut;
      this.pos = pos;
      this.accuracity = accuracity;
    }

    public override PhysicEventType Type { get { return PhysicEventType.PassageLine; } }

    public override bool Check(PCar car) {
      if (Math.Abs((car.LastPos - pos).Dot(normal)) > accuracity) {
        if (Math.Abs((car.Pos - pos).Dot(normal)) < accuracity) {
          return true;
        }

        return Math.Sign((car.Pos - pos).Dot(normal)) != Math.Sign((car.LastPos - pos).Dot(normal));
      }

      return false;
    }

    public override IPhysicEvent Copy() {
      return new PassageLineEvent(normal, pos, accuracity);
    }
  }

  public class OutLineEvent : PhysicEventBase {
    private Vector normal;
    private Vector pos;
    private double accuracity;

    public OutLineEvent(Vector normalOut, Vector pos, double accuracity = 0) {
      this.normal = normalOut;
      this.pos = pos;
      this.accuracity = accuracity;
    }

    public override PhysicEventType Type { get { return PhysicEventType.OutLine; } }

    public bool OutLine(PCar car) {
      return Math.Abs((car.Pos - pos).Dot(normal)) > accuracity;
    }

    public override bool Check(PCar car) {
      if (OutLine(car)) {
        if (Math.Abs((car.LastPos - pos).Dot(normal)) < accuracity) {
          return true;
        }

        return Math.Sign((car.Pos - pos).Dot(normal)) != Math.Sign((car.LastPos - pos).Dot(normal));
      }

      return false;
    }

    public override IPhysicEvent Copy() {
      return new OutLineEvent(normal, pos, accuracity);
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
    private double accuracy;

    public AngleReachEvent(double angle, double accuracyAngleRad = Math.PI/64) {
      this.angle = angle;
      this.accuracy = accuracyAngleRad;
    }

    public override PhysicEventType Type { get { return PhysicEventType.AngleReach; } }

    public override bool Check(PCar car) {
      double angleDeviation = angle.AngleDeviation(car.Angle);
      return Math.Abs(angleDeviation) < accuracy && Math.Abs(car.WheelTurn) < wheelTurnChangePerTick && car.AngularSpeed < rotationFrictionFactor;
    }

    public override IPhysicEvent Copy() {
      return new AngleReachEvent(angle, accuracy);
    }
  }

  public class SpeedReachEvent : PhysicEventBase {
    private double accuracyRadiance;

    public SpeedReachEvent(double accuracyAngleRad = Math.PI/64) {
      this.accuracyRadiance = accuracyAngleRad;
    }

    public override PhysicEventType Type { get { return PhysicEventType.SpeedReach; } }

    public override bool Check(PCar car) {
      return car.Speed.Normalize().Cross(car.Dir).LessDotWithAngle(accuracyRadiance);
    }

    public override IPhysicEvent Copy() {
      return new SpeedReachEvent(accuracyRadiance);
    }
  }

  public class MapCrashEvent : PhysicEventBase {
    public MapCrashEvent() {
    }

    public override PhysicEventType Type { get { return PhysicEventType.MapCrash; } }

    public override bool Check(PCar car) {
      CollisionRect carRect = new CollisionRect(car);

      List<CollisionInfo> collisions = CollisionDetector.CollisionsWithMap(carRect);

      if (!collisions.HasCollision()) {
        return false;
      }

      foreach (CollisionInfo info in collisions) {
        if (!info.CollisionDeletected) {
          continue;
        }

        Vector normal = info.NormalObj1;
        if (car.Dir.Dot(normal) > -1.0e-2) {
          continue;
        }

        checkInfo = new Tuple<Vector, Vector>(info.Point, normal);
        return true;
      }

      return false;
    }

    public override IPhysicEvent Copy() {
      return new MapCrashEvent();
    }
  }

  public class ObjectsCrashEvent : PhysicEventBase {
    private readonly List<ICollisionObject> collisionObjects;

    public ObjectsCrashEvent(List<ICollisionObject> collisionObjects) {
      this.collisionObjects = collisionObjects;
    }

    public override PhysicEventType Type { get { return PhysicEventType.ObjectsCrash; } }

    public override bool Check(PCar car) {
      CollisionRect carRect = new CollisionRect(car);

      List<CollisionInfo> collisions = CollisionDetector.CheckCollision(carRect, collisionObjects);

      if (!collisions.HasCollision()) {
        return false;
      }

      foreach (CollisionInfo info in collisions) {
        if (!info.CollisionDeletected) {
          continue;
        }

        Vector normal = info.NormalObj1;
        if (car.Dir.Dot(normal) > -1.0e-2) {
          continue;
        }

        checkInfo = new Tuple<Vector, Vector>(info.Point, normal);
        return true;
      }

      return false;
    }

    public override IPhysicEvent Copy() {
      return new ObjectsCrashEvent(collisionObjects);
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

    public override IPhysicEvent Copy() {
      return new IntersectOilStickEvent(world);
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

    public override IPhysicEvent Copy() {
      return new PassageTileEvent(tile);
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

    public override IPhysicEvent Copy() {
      return new OutFromTileEvent(tile);
    }
  }
}
