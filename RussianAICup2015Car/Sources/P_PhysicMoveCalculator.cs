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
      HashSet<IPhysicEvent> events = calculateEvents(idealPos, dirMove, idealDir);

      double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));

      Move result = new Move();
      result.EnginePower = 1.0;
      result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, Math.Atan2(dirMove.Y, dirMove.X), speedSign);

      Vector dir = new Vector(dirMove.X, dirMove.Y);

      if (events.ComeContaints(PhysicEventType.PassageLine)) {
        IPhysicEvent passageLine = events.GetEvent(PhysicEventType.PassageLine);
        IPhysicEvent speedReach = events.GetEvent(PhysicEventType.SpeedReach);

        Vector posSpeedReach = null != speedReach ? speedReach.CarCome.Pos : null;

        if (null == posSpeedReach || (posSpeedReach - idealPos).Dot(dir) > lineCount * oneLineWidth) {
          result.IsBrake = car.Speed() > Constant.MinBrakeSpeed;
        }

        int tickToSpeedReach = (null != speedReach) ? speedReach.TickCome : maxIterationCount;
        bool magnifiedPos = tickToSpeedReach < passageLine.TickCome;

        double idealAngle = magnifiedPos ? car.GetAbsoluteAngleTo(idealPos.X, idealPos.Y) : Math.Atan2(idealDir.Y, idealDir.X);
        result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, idealAngle, speedSign);
      }

      if (events.ComeContaints(PhysicEventType.MapCrash)) {
        IPhysicEvent passageLine = events.GetEvent(PhysicEventType.PassageLine);
        IPhysicEvent mapCrash = events.GetEvent(PhysicEventType.MapCrash);

        int tickToPassageLine = (null != passageLine) ? passageLine.TickCome : maxIterationCount;

        if (mapCrash.TickCome < tickToPassageLine) {
          PCar physicCar = mapCrash.CarCome;
          Vector sideNormal = mapCrash.infoCome as Vector;
          Logger.instance.Assert(null != sideNormal, "Can't get side normal");

          double angle = dir.Angle.AngleDeviation(sideNormal.Angle);
          result.WheelTurn = car.WheelTurn - speedSign * Math.Sign(angle) * game.CarWheelTurnChangePerTick;

          bool isStrongParallel = Math.Abs(Vector.sincos(car.Angle).Dot(sideNormal)) < Math.Sin(Math.PI / 18);//10 degrees
          if (isStrongParallel) {
            result.WheelTurn = 0;
          }

          bool isParallel = Math.Abs(Vector.sincos(car.Angle).Dot(sideNormal)) < Math.Sin(Math.PI / 9);//20 degrees
          isParallel |= Math.Abs(physicCar.Dir.Dot(sideNormal)) < Math.Sin(Math.PI / 9);//20 degrees

          int ticksToZeroEnginePower = (int)(car.EnginePower / game.CarEnginePowerChangePerTick);
          if (!isParallel && speedSign > 0 && mapCrash.TickCome < ticksToZeroEnginePower) {
            result.IsBrake = car.Speed() > Constant.MinBrakeSpeed;
            result.EnginePower = 0;
          }
        }
      }

      return result;
    }

    private HashSet<IPhysicEvent> calculateEvents(Vector idealPos, TileDir dirMove, Vector idealDir) {
      HashSet<IPhysicEvent> rotateEvents = calculateRotateEvents(idealPos, dirMove, idealDir);
      HashSet<IPhysicEvent> forwardEvents = calculateForwardEvents(idealPos, dirMove, idealDir);

      HashSet<IPhysicEvent> result = new HashSet<IPhysicEvent>(rotateEvents);
      if (forwardEvents.ComeContaints(PhysicEventType.MapCrash)) {
        result.Add(forwardEvents.GetEvent(PhysicEventType.MapCrash));
      }

      return result;
    }

    private HashSet<IPhysicEvent> calculateRotateEvents(Vector idealPos, TileDir dirMove, Vector idealDir) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new PassageLineEvent(dirMove, idealPos),
        new AngleReachEvent(idealDir.Angle),
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

      if (pEvents.ComeContaints(PhysicEventType.AngleReach) && !pEvents.Containts(PhysicEventType.SpeedReach)) {
        pEvents.Add(new SpeedReachEvent());
      }

      return pEvents.ComeContaints(PhysicEventType.PassageLine) && pEvents.ComeContaints(PhysicEventType.SpeedReach);
    }

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
