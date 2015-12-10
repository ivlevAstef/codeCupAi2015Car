using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public class MovingCalculator {
    private static readonly int maxCheckRotateIterationCount = 80;
    private static readonly int maxIterationCount = 180;
    private static readonly int maxCheckCrashIterationCount = 28;//800/28 = 28

    private Car car;
    private Game game;
    private World world;

    public void setupEnvironment(Car car, Game game, World world) {
      this.car = car;
      this.game = game;
      this.world = world;
    }

    public Move calculateMove(Vector idealPos, TileDir dirMove, Vector idealDir) {
      double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));

      Move result = new Move();
      result.EnginePower = 1.0;
      result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, car.GetAbsoluteAngleTo(idealPos.X, idealPos.Y), speedSign);

      move(result, idealPos, dirMove, idealDir);
      avoidSideCrash(result, idealPos, dirMove, idealDir);
      return result;
    }


    public Move calculateTurn(Vector idealPos, TileDir dirMove, Vector idealDir) {
      double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));

      Move result = new Move();
      result.EnginePower = 1.0;
      result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, car.GetAbsoluteAngleTo(idealPos.X, idealPos.Y), speedSign);

      turn(result, idealPos, dirMove, idealDir);
      avoidSideCrash(result, idealPos, dirMove, idealDir);
      return result;
    }

    private void move(Move moveResult, Vector idealPos, TileDir dirMove, Vector idealDir) {
      PCar iterCar = new PCar(car, game);
      iterCar.setEnginePower(1);

      HashSet<IPhysicEvent> events = calculateForwardEvents(idealPos, dirMove, idealDir, false);

      if (events.ComeContaints(PhysicEventType.MapCrash)) {
        double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));
        moveResult.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, Math.Atan2(idealDir.Y, idealDir.X), speedSign);
      }
    }

    private void turn(Move moveResult, Vector idealPos, TileDir dirMove, Vector idealDir) {
      PCar iterCar = new PCar(car, game);
      iterCar.setEnginePower(1);

      Vector endPoint = new TilePos(idealPos.X, idealPos.Y).ToVector(1 - dirMove.X, 1 - dirMove.Y);
      endPoint = endPoint + new Vector(dirMove.X, dirMove.Y) * game.TrackTileMargin;

      Vector dir = new Vector(dirMove.X, dirMove.Y);
      for (int i = 0, ticksCount = 0; i < 3; i++) {
        IPhysicEvent mapCrash = calculateRotateMapCrashEvents(iterCar, idealPos, dirMove, idealDir, false);

        if (null == mapCrash) {
          double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));

          HashSet<IPhysicEvent> events = calculateRotateEvents(iterCar, idealPos, dirMove, idealDir, false);

          IPhysicEvent passageLine = events.ComeContaints(PhysicEventType.PassageLine) ? events.GetEvent(PhysicEventType.PassageLine) : null;
          IPhysicEvent speedReach = events.ComeContaints(PhysicEventType.SpeedReach) ? events.GetEvent(PhysicEventType.SpeedReach) : null;

          if (null != passageLine) {
            int speedReachTick = null != speedReach ? speedReach.TickCome : maxIterationCount;

            if (speedReachTick - passageLine.TickCome > ticksCount + passageLine.TickCome) {
              moveResult.IsBrake = car.Speed() > Constant.MinBrakeSpeed;
              moveResult.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, Math.Atan2(-idealDir.Y, -idealDir.X), speedSign);
            }
          }

          if (0 == ticksCount) {
            moveResult.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, Math.Atan2(idealDir.Y, idealDir.X), speedSign);
          }

          break;
        }

        Tuple<Vector, Vector> crashInfo = mapCrash.infoCome as Tuple<Vector, Vector>;
        Logger.instance.Assert(null != crashInfo, "Can't get crash info");

        MoveToPoint mover = new MoveToPoint(idealPos);
        double distance = (endPoint - crashInfo.Item1).Length;
        int ticksCountSave = ticksCount;
        while (distance > 0 && ticksCountSave + mapCrash.TickCome > ticksCount) {
          double speedL = iterCar.Speed.Length;
          double decreminant = 4 * speedL * speedL + 8 * iterCar.Assel * distance;

          int ticks = (int)Math.Max(1, (- 2 * speedL + Math.Sqrt(decreminant)) / (2 * iterCar.Assel));
          ticks = Math.Min(ticksCountSave + mapCrash.TickCome - ticksCount, ticks);

          if (ticksCount + ticks > maxIterationCount) {/*save function*/
            i = 3;
            break;
          }

          Vector lastPos = iterCar.Pos;
          mover.Iteration(iterCar, ticks);
          distance -= (iterCar.Pos - lastPos).Length;

          ticksCount += ticks;
        }
      }
    }

    private void avoidSideCrash(Move moveResult, Vector idealPos, TileDir dirMove, Vector idealDir) {
      HashSet<IPhysicEvent> events = calculateForwardEvents(idealPos, dirMove, idealDir, moveResult.IsBrake);

      IPhysicEvent passageLine = events.ComeContaints(PhysicEventType.PassageLine) ? events.GetEvent(PhysicEventType.PassageLine) : null;
      IPhysicEvent mapCrash = events.ComeContaints(PhysicEventType.MapCrash) ? events.GetEvent(PhysicEventType.MapCrash) : null;

      if (null != mapCrash) {
        int tickToPassageLine = (null != passageLine) ? passageLine.TickCome : maxIterationCount;

        if (mapCrash.TickCome < tickToPassageLine) {
          double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));
          Vector dir = new Vector(dirMove.X, dirMove.Y);
          PCar physicCar = mapCrash.CarCome;
          Tuple<Vector, Vector> crashInfo = mapCrash.infoCome as Tuple<Vector, Vector>;
          Logger.instance.Assert(null != crashInfo, "Can't get crash info");
          Vector sideNormal = crashInfo.Item2;

          double angle = dir.Angle.AngleDeviation(sideNormal.Angle);
          if (Vector.sincos(car.Angle).Dot(dir) > 0 || 0 == angle) {
            angle = car.Angle.AngleDeviation(sideNormal.Angle);
          }

          moveResult.WheelTurn = car.WheelTurn - speedSign * Math.Sign(angle) * game.CarWheelTurnChangePerTick;

          bool isParallel = Math.Abs(Vector.sincos(car.Angle).Dot(sideNormal)) < Math.Sin(Math.PI / 9);//20 degrees
          isParallel |= Math.Abs(physicCar.Dir.Dot(sideNormal)) < Math.Sin(Math.PI / 9);//20 degrees

          int ticksToZeroEnginePower = (int)(car.EnginePower / game.CarEnginePowerChangePerTick);
          if (!isParallel && speedSign > 0 && mapCrash.TickCome < ticksToZeroEnginePower) {
            moveResult.IsBrake = car.Speed() > Constant.MinBrakeSpeed;
          }
        }
      }
    }

    /// Rotate MapCrash
    private IPhysicEvent calculateRotateMapCrashEvents(PCar iterCar, Vector idealPos, TileDir dirMove, Vector idealDir, bool isBrake) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new PassageLineEvent(dirMove, idealPos),
        new MapCrashEvent(additionalSideToTileByDir(iterCar, dirMove, new TilePos(idealPos.X, idealPos.Y)))
      };

      PCar physicCar = new PCar(iterCar);
      physicCar.setEnginePower(1.0);
      physicCar.setBrake(isBrake);

      double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);
      PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(idealAngle), pEvents, calculateRotateMapCrashEventCheckEnd);

      return pEvents.ComeContaints(PhysicEventType.MapCrash) ? pEvents.GetEvent(PhysicEventType.MapCrash) : null;
    }

    private bool calculateRotateMapCrashEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxCheckRotateIterationCount) {
        return true;
      }

      return pEvents.ComeContaints(PhysicEventType.PassageLine) || pEvents.ComeContaints(PhysicEventType.MapCrash);
    }

    /// Rotate
    private HashSet<IPhysicEvent> calculateRotateEvents(PCar iterCar, Vector idealPos, TileDir dirMove, Vector idealDir, bool isBrake) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new PassageLineEvent(dirMove, idealPos),
        new AngleReachEvent(idealDir.Angle),
      };

      PCar physicCar = new PCar(iterCar);
      physicCar.setEnginePower(1.0);
      physicCar.setBrake(isBrake);

      double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);
      PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(idealAngle), pEvents, calculateRotateEventCheckEnd);

      if (!pEvents.Containts(PhysicEventType.SpeedReach)) {
        pEvents.Add(new SpeedReachEvent());
      }

      return pEvents;
    }

    private bool calculateRotateEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxIterationCount) {
        return true;
      }

      if (pEvents.ComeContaints(PhysicEventType.AngleReach) && !pEvents.Containts(PhysicEventType.SpeedReach)) {
        pEvents.Add(new SpeedReachEvent());
      }

      return pEvents.ComeContaints(PhysicEventType.SpeedReach);
    }

    /// Forward
    private HashSet<IPhysicEvent> calculateForwardEvents(Vector idealPos, TileDir dirMove, Vector idealDir, bool isBrake) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new PassageLineEvent(dirMove, idealPos),
        new MapCrashEvent(null)
      };

      PCar physicCar = new PCar(car, game);
      physicCar.setEnginePower(1.0);
      physicCar.setWheelTurn(0);
      physicCar.setBrake(isBrake);

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
    private List<ICollisionObject> additionalSideToTileByDir(PCar pCar, TileDir dir, TilePos pos) {
      CollisionRect rect = new CollisionRect(pCar);

      TilePos current = new TilePos(pCar.Pos.X, pCar.Pos.Y);

      int distance = (pos - current).X * dir.X + (pos - current).Y * dir.Y;

      List<ICollisionObject> result = new List<ICollisionObject>();
      for (int i = 0; i < distance; i++) {
        CollisionSide sideLeft = new CollisionSide(current + dir * i, dir.PerpendicularLeft());
        CollisionSide sideRight = new CollisionSide(current + dir * i, dir.PerpendicularRight());

        if (null == CollisionDetector.CheckCollision(rect, sideLeft)) {
          result.Add(sideLeft);
        }

        if (null == CollisionDetector.CheckCollision(rect, sideRight)) {
          result.Add(sideRight);
        }
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
