using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public class MovingCalculator {
    private const int maxIterationCount = 250;
    private const int maxCheckCrashIterationCount = 75;
    private double oneLineWidth;

    private Car car;
    private Game game;
    private World world;

    public void setupEnvironment(Car car, Game game, World world) {
      this.car = car;
      this.game = game;
      this.world = world;
      oneLineWidth = (game.TrackTileSize - 2 * game.TrackTileMargin) / 4;
    }

    public Move calculateMove(Vector idealPos, TileDir dirMove, Vector idealDir, double lineCount = 1.25) {
      double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));

      Move result = new Move();
      result.EnginePower = 1.0;
      result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, car.GetAbsoluteAngleTo(idealPos.X, idealPos.Y), speedSign);

      move(result, idealPos, dirMove, idealDir, lineCount);
      avoidSideCrash(result, idealPos, dirMove, idealDir);
      return result;
    }

    private void move(Move moveResult, Vector idealPos, TileDir dirMove, Vector idealDir, double lineCount = 1.25) {
      HashSet<IPhysicEvent> events = calculateRotateEvents(idealPos, dirMove, idealDir);
      HashSet<IPhysicEvent> eventsBrake = calculateRotateWithBrakeEvents(idealDir);

      IPhysicEvent passageLine = events.ComeContaints(PhysicEventType.PassageLine) ? events.GetEvent(PhysicEventType.PassageLine) : null;
      IPhysicEvent speedReach = eventsBrake.ComeContaints(PhysicEventType.SpeedReach) ? eventsBrake.GetEvent(PhysicEventType.SpeedReach) : null;
      IPhysicEvent angleReach = eventsBrake.ComeContaints(PhysicEventType.AngleReach) ? eventsBrake.GetEvent(PhysicEventType.AngleReach) : null;

      if (null != passageLine) {
        double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));
        Vector dir = new Vector(dirMove.X, dirMove.Y);

        Vector posSpeedReach = null != speedReach ? speedReach.CarCome.Pos : null;

        IPhysicEvent mapCrash = null;
        if (null == posSpeedReach || (posSpeedReach - idealPos).Dot(dir) > lineCount * oneLineWidth) {
          moveResult.IsBrake = car.Speed() > Constant.MinBrakeSpeed;
          mapCrash = calculateRotateMapCrashEvents(idealPos, dirMove, idealDir, true);
        } else {
          mapCrash = calculateRotateMapCrashEvents(idealPos, dirMove, idealDir, false);
        }

        int tickToMapCrash = (null != mapCrash) ? mapCrash.TickCome : maxIterationCount;
        if (tickToMapCrash > passageLine.TickCome) {
          if (null != mapCrash || Math.Abs(idealDir.Cross(new Vector(dirMove.X, dirMove.Y))) > 1.0e-3) {
            moveResult.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, Math.Atan2(idealDir.Y, idealDir.X), speedSign);
          }
        }
      }
    }

    private void avoidSideCrash(Move moveResult, Vector idealPos, TileDir dirMove, Vector idealDir) {
      HashSet<IPhysicEvent> events = calculateForwardEvents(idealPos, dirMove, idealDir);

      IPhysicEvent passageLine = events.ComeContaints(PhysicEventType.PassageLine) ? events.GetEvent(PhysicEventType.PassageLine) : null;
      IPhysicEvent mapCrash = events.ComeContaints(PhysicEventType.MapCrash) ? events.GetEvent(PhysicEventType.MapCrash) : null;

      if (null != mapCrash) {
        int tickToPassageLine = (null != passageLine) ? passageLine.TickCome : maxIterationCount;

        if (mapCrash.TickCome < tickToPassageLine) {
          double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));

          PCar physicCar = mapCrash.CarCome;
          Vector sideNormal = mapCrash.infoCome as Vector;
          Logger.instance.Assert(null != sideNormal, "Can't get side normal");

          double angleSign = Vector.sincos(car.Angle).Dot(new Vector(dirMove.X, dirMove.Y));
          double angle = angleSign * car.Angle.AngleDeviation(sideNormal.Angle);

          bool isStrongParallel = Math.Abs(Vector.sincos(car.Angle).Dot(sideNormal)) < Math.Sin(Math.PI / 18);//10 degrees
          if (!isStrongParallel) {
            moveResult.WheelTurn = car.WheelTurn - speedSign * Math.Sign(angle) * game.CarWheelTurnChangePerTick;
          }

          bool isParallel = Math.Abs(Vector.sincos(car.Angle).Dot(sideNormal)) < Math.Sin(Math.PI / 9);//20 degrees
          isParallel |= Math.Abs(physicCar.Dir.Dot(sideNormal)) < Math.Sin(Math.PI / 9);//20 degrees

          int ticksToZeroEnginePower = (int)(car.EnginePower / game.CarEnginePowerChangePerTick);
          if (!isParallel && speedSign > 0 && mapCrash.TickCome < ticksToZeroEnginePower) {
            moveResult.IsBrake = car.Speed() > Constant.MinBrakeSpeed;
            moveResult.EnginePower = 0;
          }
        }
      }
    }

    /// Rotate MapCrash
    private IPhysicEvent calculateRotateMapCrashEvents(Vector idealPos, TileDir dirMove, Vector idealDir, bool useBrake) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new PassageLineEvent(dirMove, idealPos),
        new MapCrashEvent(additionalSideToTileByDir(dirMove, new TilePos(idealPos.X - dirMove.X, idealPos.Y - dirMove.Y)))
      };

      PCar physicCar = new PCar(car, game);
      physicCar.setEnginePower(1.0);

      double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);

      if (useBrake) {
        PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(idealAngle), pEvents, calculateRotateMapCrashEventWithBrakeCheckEnd);
      } else {
        PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(idealAngle), pEvents, calculateRotateMapCrashEventCheckEnd);
      }

      return pEvents.ComeContaints(PhysicEventType.MapCrash) ? pEvents.GetEvent(PhysicEventType.MapCrash) : null; ;
    }

    private bool calculateRotateMapCrashEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxIterationCount) {
        return true;
      }

      return pEvents.ComeContaints(PhysicEventType.PassageLine) || pEvents.ComeContaints(PhysicEventType.MapCrash);
    }

    private bool calculateRotateMapCrashEventWithBrakeCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxIterationCount) {
        return true;
      }

      physicCar.setBrake(physicCar.Speed.Length > Constant.MinBrakeSpeed);
      return pEvents.ComeContaints(PhysicEventType.PassageLine) || pEvents.ComeContaints(PhysicEventType.MapCrash);
    }

    /// Rotate
    private HashSet<IPhysicEvent> calculateRotateEvents(Vector idealPos, TileDir dirMove, Vector idealDir) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new PassageLineEvent(dirMove, idealPos)
      };

      PCar physicCar = new PCar(car, game);
      physicCar.setEnginePower(1.0);

      double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);
      PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(idealAngle), pEvents, calculateRotateEventCheckEnd);

      return pEvents;
    }

    private bool calculateRotateEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxIterationCount) {
        return true;
      }

      return pEvents.ComeContaints(PhysicEventType.PassageLine);
    }

    /// Rotate With Brake
    private HashSet<IPhysicEvent> calculateRotateWithBrakeEvents(Vector idealDir) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new AngleReachEvent(idealDir.Angle),
      };

      PCar physicCar = new PCar(car, game);
      physicCar.setEnginePower(1.0);

      double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);
      PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(idealAngle), pEvents, calculateRotateWithBrakeEventCheckEnd);

      if (!pEvents.Containts(PhysicEventType.SpeedReach)) {
        pEvents.Add(new SpeedReachEvent());
      }

      return pEvents;
    }

    private bool calculateRotateWithBrakeEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxIterationCount) {
        return true;
      }

      if (pEvents.ComeContaints(PhysicEventType.AngleReach) && !pEvents.Containts(PhysicEventType.SpeedReach)) {
        pEvents.Add(new SpeedReachEvent());
      }

      if (!pEvents.ComeContaints(PhysicEventType.AngleReach)) {
        physicCar.setBrake(physicCar.Speed.Length > Constant.MinBrakeSpeed);
      }

      return pEvents.ComeContaints(PhysicEventType.SpeedReach);
    }


    /// Forward
    private HashSet<IPhysicEvent> calculateForwardEvents(Vector idealPos, TileDir dirMove, Vector idealDir) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new PassageLineEvent(dirMove, idealPos),
        new MapCrashEvent(additionalSideToTileByDir(dirMove, new TilePos(idealPos.X - dirMove.X, idealPos.Y - dirMove.Y)))
      };

      PCar physicCar = new PCar(car, game);
      physicCar.setEnginePower(1.0);

      double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);
      double angleDeviation = idealAngle.AngleDeviation(physicCar.Angle);

      if (Math.Abs(angleDeviation) > game.CarRotationFrictionFactor) {
        physicCar.setWheelTurn(physicCar.WheelTurn + 0.5 * game.CarWheelTurnChangePerTick * Math.Sign(angleDeviation));
      }

      PhysicEventsCalculator.calculateEvents(physicCar, new MoveWithOutChange(), pEvents, calculateForwardEventCheckEnd);

      return pEvents;
    }

    private bool calculateForwardEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxCheckCrashIterationCount) { 
        return true;
      }

      return pEvents.ComeContaints(PhysicEventType.PassageLine) || pEvents.ComeContaints(PhysicEventType.MapCrash);
    }


    ///Other
    private List<ICollisionObject> additionalSideToTileByDir(TileDir dir, TilePos pos) {

      TilePos current = new TilePos(car.X, car.Y);

      int distance = (pos - current).X * dir.X + (pos - current).Y * dir.Y;

      List<ICollisionObject> result = new List<ICollisionObject>();
      for (int i = 0; i < distance; i++) {
        result.Add(new CollisionSide(current + dir * i, dir.PerpendicularLeft()));
        result.Add(new CollisionSide(current + dir * i, dir.PerpendicularRight()));
      }

      return result;
    }

    private static double WheelTurnForEndZeroWheelTurn(Car car, Game game, double finalAngle, double sign) {
      PCar physicCar = new PCar(car, game);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / game.CarWheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);

      double angleDeviation = finalAngle.AngleDeviation(physicCar.Angle);

      if (Math.Abs(angleDeviation) < game.CarRotationFrictionFactor) {
        return 0;
      }

      return car.WheelTurn + /*sign **/ game.CarWheelTurnChangePerTick * Math.Sign(angleDeviation);
    }
  }
}
