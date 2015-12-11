using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public class MovingCalculator {
    private static readonly int maxCheckRotateIterationCount = 80;
    private static readonly int maxIterationCount = 180;
    private static readonly int maxCheckCrashIterationCount = 36;//800/22 = 36
    private static readonly int maxCheckMoveCrashIterationCount = 50;

    private Car car;
    private Game game;
    private World world;

    private PassageLineEvent passageLineEvent = null;
    private AngleReachEvent angleReachEvent = null;
    private ObjectsCrashEvent moverSelfMapCrashEvent = null;

    private Vector defaultPos = null;
    private double defaultAngle = 0;
    private TileDir dirMove = null;
    private TilePos currentTile = null;
    private TilePos endTile = null;

    private double enginePowerSign = 1;

    private double speedSign = 1;

    public void setupEnvironment(Car car, Game game, World world) {
      this.car = car;
      this.game = game;
      this.world = world;

      this.speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));
    }

    public void setupMapInfo(TileDir dirMove, TilePos currentTile, TilePos endTile) {
      this.dirMove = dirMove;
      this.currentTile = currentTile;
      this.endTile = endTile;
    }

    public void setupDefaultAction(Vector defaultPos) {
      this.defaultPos = defaultPos;
      this.defaultAngle = car.GetAbsoluteAngleTo(defaultPos.X, defaultPos.Y);
    }

    public void setupPassageLine(Vector pos, Vector dir) {
      passageLineEvent = new PassageLineEvent(dir, pos);
    }

    public void setupAngleReach(Vector angleDir) {
      angleReachEvent = new AngleReachEvent(angleDir.Angle);
    }

    public void setupSelfMapCrash(Dictionary<TilePos, TileDir[]> tilesInfo) {
      moverSelfMapCrashEvent = new ObjectsCrashEvent(objectsByTilesAndEdgesInfo(tilesInfo));
    }

    public void useBackward(bool use = true) {
      this.enginePowerSign = use ? -1 : 1;
    }

    public Move calculateMove() {
      Move result = new Move();
      result.EnginePower = enginePowerSign;
      result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, defaultAngle, speedSign);

      move(result);
      avoidSideCrash(result);
      return result;
    }

    public Move calculateTurn(Vector needDirAngle) {
      Move result = new Move();
      result.EnginePower = enginePowerSign;
      result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, defaultAngle, speedSign);

      turn(result, needDirAngle);
      avoidSideCrash(result, needDirAngle);
      return result;
    }

    private void move(Move moveResult) {
      PCar iterCar = new PCar(car, game);
      iterCar.setEnginePower(enginePowerSign);

      HashSet<IPhysicEvent> events = calculateMoveEvents(iterCar);

      if (events.ComeContaints(PhysicEventType.MapCrash)) {
        moveResult.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, Math.Atan2(dirMove.Y, dirMove.X), speedSign);
      }
    }

    private void turn(Move moveResult, Vector needDirAngle) {
      PCar iterCar = new PCar(car, game);
      iterCar.setEnginePower(enginePowerSign);

      Vector endPoint = endTile.ToVector(1 - dirMove.X, 1 - dirMove.Y);
      endPoint = endPoint + new Vector(dirMove.X, dirMove.Y) * game.TrackTileMargin;

      Vector dir = new Vector(dirMove.X, dirMove.Y);
      for (int i = 0, ticksCount = 0; i < 3; i++) {
        IPhysicEvent mapCrash = calculateTurnMapCrashEvents(iterCar, needDirAngle);

        if (null == mapCrash) {
          double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));

          HashSet<IPhysicEvent> events = calculateTurnEvents(iterCar, needDirAngle);

          IPhysicEvent passageLine = events.ComeContaints(PhysicEventType.PassageLine) ? events.GetEvent(PhysicEventType.PassageLine) : null;
          IPhysicEvent speedReach = events.ComeContaints(PhysicEventType.SpeedReach) ? events.GetEvent(PhysicEventType.SpeedReach) : null;

          if (null != passageLine) {
            int speedReachTick = null != speedReach ? speedReach.TickCome : maxIterationCount;

            if (speedReachTick > ticksCount + passageLine.TickCome) {
              moveResult.IsBrake = car.Speed() > Constant.MinBrakeSpeed;
            }
          }

          if (0 == ticksCount) {
            moveResult.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, needDirAngle.Angle, speedSign);
          }

          break;
        }

        Tuple<Vector, Vector> crashInfo = mapCrash.infoCome as Tuple<Vector, Vector>;
        Logger.instance.Assert(null != crashInfo, "Can't get crash info");

        MoveToPoint mover = new MoveToPoint(defaultPos);
        double distance = (endPoint - crashInfo.Item1).Length;
        int ticksCountSave = ticksCount;
        while (distance > 0 && ticksCountSave + mapCrash.TickCome > ticksCount) {
          double speedL = iterCar.Speed.Length;
          double decreminant = 4 * speedL * speedL + 8 * Math.Abs(iterCar.Assel) * distance;

          int ticks = (int)Math.Max(1, (- 2 * speedL + Math.Sqrt(decreminant)) / (2 * Math.Abs(iterCar.Assel)));
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

    private void avoidSideCrash(Move moveResult, Vector needDirAngle = null) {
      HashSet<IPhysicEvent> events = calculateAvoidMapCrashEvents(moveResult);

      IPhysicEvent passageLine = events.ComeContaints(PhysicEventType.PassageLine) ? events.GetEvent(PhysicEventType.PassageLine) : null;
      IPhysicEvent mapCrash = events.ComeContaints(PhysicEventType.MapCrash) ? events.GetEvent(PhysicEventType.MapCrash) : null;

      if (null != mapCrash) {
        int tickToPassageLine = (null != passageLine) ? passageLine.TickCome : maxIterationCount;

        if (mapCrash.TickCome < tickToPassageLine) {
          Vector dir = new Vector(dirMove.X, dirMove.Y);

          PCar physicCar = mapCrash.CarCome;
          Tuple<Vector, Vector> crashInfo = mapCrash.infoCome as Tuple<Vector, Vector>;
          Logger.instance.Assert(null != crashInfo, "Can't get crash info");
          Vector sideNormal = crashInfo.Item2;

          double angle = dir.Angle.AngleDeviation(sideNormal.Angle);
          if (Vector.sincos(car.Angle).Dot(dir) > 0 || 0 == angle) {
            angle = car.Angle.AngleDeviation(sideNormal.Angle);
          }

          bool isStrongParallel = Math.Abs(Vector.sincos(car.Angle).Dot(sideNormal)) < Math.Sin(Math.PI / 18);//10 degrees
          isStrongParallel &= Math.Abs(physicCar.Dir.Dot(sideNormal)) < Math.Sin(Math.PI / 9);//10 degrees

          bool notCurrentTurnSide = null != needDirAngle && sideNormal.Dot(needDirAngle) < 0;

          if (!isStrongParallel || notCurrentTurnSide) {
            moveResult.WheelTurn = car.WheelTurn - speedSign * Math.Sign(angle) * game.CarWheelTurnChangePerTick;
          } else {
            moveResult.WheelTurn = 0;
          }

          bool isParallel = Math.Abs(Vector.sincos(car.Angle).Dot(sideNormal)) < Math.Sin(Math.PI / 9);//20 degrees
          isParallel |= Math.Abs(physicCar.Dir.Dot(sideNormal)) < Math.Sin(Math.PI / 9);//20 degrees

          int ticksToZeroEnginePower = (int)(Math.Abs(car.EnginePower) / game.CarEnginePowerChangePerTick);
          if (!isParallel && speedSign > 0 && mapCrash.TickCome < ticksToZeroEnginePower) {
            moveResult.IsBrake = car.Speed() > Constant.MinBrakeSpeed;
          }
        }
      }
    }

    /// Turn MapCrash
    private IPhysicEvent calculateTurnMapCrashEvents(PCar iterCar, Vector needDirAngle) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        passageLineEvent
      };

      if (null != moverSelfMapCrashEvent) {
        pEvents.Add(moverSelfMapCrashEvent);
      }

      PCar physicCar = new PCar(iterCar);
      PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(needDirAngle.Angle), pEvents, calculateTurnMapCrashEventCheckEnd);

      return pEvents.ComeContaints(PhysicEventType.ObjectsCrash) ? pEvents.GetEvent(PhysicEventType.ObjectsCrash) : null;
    }

    private bool calculateTurnMapCrashEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxCheckRotateIterationCount) {
        return true;
      }

      return pEvents.ComeContaints(PhysicEventType.PassageLine) || pEvents.ComeContaints(PhysicEventType.ObjectsCrash);
    }

    /// Turn
    private HashSet<IPhysicEvent> calculateTurnEvents(PCar iterCar, Vector needDirAngle) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        passageLineEvent,
        angleReachEvent
      };

      PCar physicCar = new PCar(iterCar);
      PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(needDirAngle.Angle), pEvents, calculateTurnEventCheckEnd);

      if (!pEvents.Containts(PhysicEventType.SpeedReach)) {
        pEvents.Add(new SpeedReachEvent());
      }

      return pEvents;
    }

    private bool calculateTurnEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxIterationCount) {
        return true;
      }

      if (pEvents.ComeContaints(PhysicEventType.AngleReach) && !pEvents.Containts(PhysicEventType.SpeedReach)) {
        pEvents.Add(new SpeedReachEvent());
      }

      return pEvents.ComeContaints(PhysicEventType.SpeedReach);
    }

    ///Move
    private HashSet<IPhysicEvent> calculateMoveEvents(PCar iterCar) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new MapCrashEvent()
      };

      if (null != moverSelfMapCrashEvent) {
        pEvents.Add(moverSelfMapCrashEvent);
      }

      PCar physicCar = new PCar(iterCar);
      PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(Math.Atan2(dirMove.Y, dirMove.X)), pEvents, calculateMoveEventCheckEnd);

      return pEvents;
    }

    private bool calculateMoveEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxCheckMoveCrashIterationCount) {
        return true;
      }

      return pEvents.ComeContaints(PhysicEventType.MapCrash);
    }

    /// avoid map crash
    private HashSet<IPhysicEvent> calculateAvoidMapCrashEvents(Move currentMove) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new MapCrashEvent()
      };

      if (null != passageLineEvent) {
        pEvents.Add(passageLineEvent);
      }

      PCar physicCar = new PCar(car, game);
      physicCar.setEnginePower(currentMove.EnginePower);
      physicCar.setWheelTurn(currentMove.WheelTurn);
      physicCar.setBrake(currentMove.IsBrake);

      PhysicEventsCalculator.calculateEvents(physicCar, new MoveWithOutChange(), pEvents, calculateAvoidSideCrashEventCheckEnd);

      return pEvents;
    }

    private bool calculateAvoidSideCrashEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxCheckCrashIterationCount) { 
        return true;
      }

      return pEvents.ComeContaints(PhysicEventType.PassageLine) || pEvents.ComeContaints(PhysicEventType.MapCrash);
    }

    ///Other
    private List<ICollisionObject> objectsByTilesAndEdgesInfo(Dictionary<TilePos, TileDir[]> tilesInfo) {
      CollisionRect carRect = new CollisionRect(car);

      List<ICollisionObject> result = new List<ICollisionObject>();

      if (null != tilesInfo) {
        foreach (TilePos pos in tilesInfo.Keys) {
          foreach (TileDir dir in tilesInfo[pos]) {
            ICollisionObject obj = null;
            if (dir.Correct()) {
              obj = new CollisionSide(pos, dir);
            } else {
              obj = new CollisionCircle(pos, dir);
            }

            if (null == CollisionDetector.CheckCollision(carRect, obj)) {
              result.Add(obj);
            }
          }
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

      return car.WheelTurn + sign * game.CarWheelTurnChangePerTick * Math.Sign(angleDeviation);
    }
  }
}
