using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;
using RussianAICup2015Car.Sources.Visualization;

namespace RussianAICup2015Car.Sources.Physic {
  public class MovingCalculator {
    private static readonly int maxCheckRotateIterationCount = 180;
    private static readonly int maxIterationCount = 120;
    private static readonly int maxCheckCrashIterationCount = 30;
    private static readonly int maxCheckMoveCrashIterationCount = 80;

    private Car car;
    private Game game;
    private World world;
    private VisualClient vClient;

    private PassageLineEvent passageLineEvent = null;
    private OutLineEvent outLineEvent = null;
    private AngleReachEvent angleReachEvent = null;
    private ObjectsCrashEvent moverSelfMapCrashEvent = null;

    private List<Tuple<Vector, double>> additionalPoints = null;
    private Vector defaultPos = null;
    private Vector needPos = null;
    private TileDir dirMove = null;
    private TilePos currentTile = null;
    private TilePos endTile = null;

    private double enginePowerSign = 1;

    private double speedSign = 1;

    public void setupEnvironment(Car car, Game game, World world, VisualClient vClient) {
      this.car = car;
      this.game = game;
      this.world = world;
      this.vClient = vClient;

      this.speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));
    }

    public void setupMapInfo(TileDir dirMove, TilePos currentTile, TilePos endTile) {
      this.dirMove = dirMove;
      this.currentTile = currentTile;
      this.endTile = endTile;
    }

    public void setupDefaultAction(Vector defaultPos) {
      this.defaultPos = defaultPos;
      this.needPos = null;
    }

    public void setupPassageLine(Vector pos, Vector normal, double accuracity) {
      double oneLineSize = (game.TrackTileSize - 2 * game.TrackTileMargin) / 4;
      passageLineEvent = new PassageLineEvent(normal, pos, accuracity * oneLineSize);
      outLineEvent = new OutLineEvent(normal, pos, accuracity * oneLineSize);

      if (null != vClient) {
        Vector dir = normal.PerpendicularLeft();
        Vector p1 = pos + dir * 2 * game.TrackTileSize;
        Vector p2 = pos - dir * game.TrackTileSize;
        vClient.Line(p1.X, p1.Y, p2.X, p2.Y, 0xFF7F7F);

        p1 = pos + dir * 2 * game.TrackTileSize + dir.Perpendicular() * accuracity * oneLineSize;
        p2 = pos - dir * game.TrackTileSize + dir.Perpendicular() * accuracity * oneLineSize;
        vClient.Line(p1.X, p1.Y, p2.X, p2.Y, 0xFF7FFF);

        p1 = pos + dir * 2 * game.TrackTileSize - dir.Perpendicular() * accuracity * oneLineSize;
        p2 = pos - dir * game.TrackTileSize - dir.Perpendicular() * accuracity * oneLineSize;
        vClient.Line(p1.X, p1.Y, p2.X, p2.Y, 0xFF7FFF);
      }
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

    public void setupAdditionalPoints(List<Tuple<Vector, double>> additionalPoints) {
      this.additionalPoints = additionalPoints;
    }

    public Move calculateMove() {
      Move result = new Move();
      result.EnginePower = enginePowerSign;

      moveToAdditionalPoint(Math.Atan2(dirMove.Y, dirMove.X));
      result.WheelTurn = wheelTurnForCurrentState(Math.Atan2(dirMove.Y, dirMove.X));

      move(result);
      avoidSideCrash(result);
      return result;
    }

    public Move calculateTurn(Vector needDirAngle) {
      Move result = new Move();
      result.EnginePower = enginePowerSign;

      moveToAdditionalPoint(needDirAngle.Angle);
      result.WheelTurn = wheelTurnForCurrentState(needDirAngle.Angle);

      turn(result, needDirAngle);
      avoidSideCrash(result, needDirAngle);
      return result;
    }

    private void move(Move moveResult) {
      PCar physicCar = new PCar(car, game);
      physicCar.SetupVisualClient(vClient, 0x0000FF);
      physicCar.setEnginePower(enginePowerSign);

      HashSet<IPhysicEvent> events = calculateMoveEvents(physicCar);

      if (events.ComeContaints(PhysicEventType.MapCrash) || events.ComeContaints(PhysicEventType.ObjectsCrash)) {
        moveResult.WheelTurn = physicCar.WheelTurnForEndZeroWheelTurn(defaultPos, speedSign);
      }
    }

    private double wheelTurnForCurrentState(double angle) {
      if (null != needPos) {
        return new PCar(car, game).WheelTurnForEndZeroWheelTurnToPoint(needPos, angle, speedSign);
      } else {
        return new PCar(car, game).WheelTurnForEndZeroWheelTurn(defaultPos, speedSign);
      }
    }

    private void turn(Move moveResult, Vector needDirAngle) {
      PCar iterCar = new PCar(car, game);
      iterCar.SetupVisualClient(vClient, 0x0000FF);
      iterCar.setEnginePower(enginePowerSign);

      Vector endPoint = endTile.ToVector(1 - dirMove.X, 1 - dirMove.Y);
      endPoint = endPoint + new Vector(dirMove.X, dirMove.Y) * game.TrackTileMargin;

      double speedSign = Math.Sign(Vector.sincos(car.Angle).Dot(new Vector(car.SpeedX, car.SpeedY)));

      HashSet<IPhysicEvent> events = calculateTurnEvents(iterCar, needDirAngle);

      IPhysicEvent passageLine = events.ComeContaints(PhysicEventType.PassageLine) ? events.GetEvent(PhysicEventType.PassageLine) : null;
      IPhysicEvent outLine = events.ComeContaints(PhysicEventType.OutLine) ? events.GetEvent(PhysicEventType.OutLine) : null;
      IPhysicEvent speedReach = events.ComeContaints(PhysicEventType.SpeedReach) ? events.GetEvent(PhysicEventType.SpeedReach) : null;

      if (null != passageLine && null != outLine) {
        int speedReachTick = null != speedReach ? speedReach.TickCome : maxIterationCount;

        if (speedReachTick * iterCar.CalculateBrakeFactor() > outLine.TickCome) {
          moveResult.IsBrake = car.Speed() > Constant.MinBrakeSpeed;
        }
      }

      if (!hasReserveTicks(iterCar, needDirAngle)) {
        HashSet<IPhysicEvent> crashEvents = calculateTurnMapCrashEvents(iterCar, needDirAngle, moveResult.IsBrake);
        IPhysicEvent mapBrakeCrash = crashEvents.ComeContaints(PhysicEventType.ObjectsCrash) ? crashEvents.GetEvent(PhysicEventType.ObjectsCrash) : null;
        IPhysicEvent passageLineBrake = crashEvents.ComeContaints(PhysicEventType.PassageLine) ? crashEvents.GetEvent(PhysicEventType.PassageLine) : null;

        //bool endMove = checkStrongParallel(mapBrakeCrash);
        int tickToZeroWheelTurn = (int)Math.Round(Math.Abs(iterCar.WheelTurn / game.CarWheelTurnChangePerTick));
        bool nearEndAndCrash = (null != mapBrakeCrash && null != passageLine && mapBrakeCrash.TickCome <= tickToZeroWheelTurn && passageLine.TickCome < tickToZeroWheelTurn);
        if ((null == mapBrakeCrash && null != passageLineBrake) || nearEndAndCrash) {
          moveResult.WheelTurn = new PCar(car, game).WheelTurnForEndZeroWheelTurn(needDirAngle.Angle, speedSign);
        }
      }

      if (isMovedOutFromLine(iterCar, needDirAngle)) {
        moveResult.WheelTurn = new PCar(car, game).WheelTurnForEndZeroWheelTurn(needDirAngle.Angle, speedSign);
      }
    }

    private int moveOnDistance(PCar car, Vector needDirAngle, double distance) {
      MoveToPoint mover = null;
      if (null != needPos) {
        mover = new MoveToPoint(needPos, needDirAngle.Angle);
      } else {
        mover = new MoveToPoint(defaultPos);
      }

      double speedL = car.Speed.Length;

      int addTick = 1;
      if (speedL > 1.0e-3) {
        addTick = (int)Math.Max(1, Math.Min(0.5 * distance / speedL, 1024));
      } else {
        addTick = 2;
      }

      mover.Iteration(car, addTick);
      return addTick;
    }

    private bool hasReserveTicks(PCar iterCar, Vector needDirAngle) {
      MoveToPoint mover = null;
      if (null != needPos) {
        mover = new MoveToPoint(needPos, needDirAngle.Angle);
      } else {
        mover = new MoveToPoint(defaultPos);
      }

      PCar car = new PCar(iterCar);

      for (int tick = 0; tick < 5; tick++) {
        HashSet<IPhysicEvent> events = calculateTurnOutLineEvents(car, needDirAngle);

        if (events.ComeContaints(PhysicEventType.OutLine)) {
          return false;
        }

        mover.Iteration(car, 1);
      }

      return true;
    }

    private bool isMovedOutFromLine(PCar iterCar, Vector needDirAngle) {
      if (outLineEvent.OutLine(iterCar)) {
        return false;
      }

      MoveToPoint mover = null;
      if (null != needPos) {
        mover = new MoveToPoint(needPos, needDirAngle.Angle);
      } else {
        mover = new MoveToPoint(defaultPos);
      }

      PCar carToAngle = new PCar(iterCar);
      MoveToAngleFunction moveToAngle = new MoveToAngleFunction(needDirAngle.Angle);
      int ticksToAngle = 0;
      for (; ticksToAngle < 100; ticksToAngle++) {
        moveToAngle.Iteration(carToAngle, 1);
        if (angleReachEvent.Check(carToAngle)) {
          break;
        }
      }

      PCar car = new PCar(iterCar);
      mover.Iteration(car, ticksToAngle);

      return outLineEvent.OutLine(car);
    }

    private void avoidSideCrash(Move moveResult, Vector needDirAngle = null) {
      HashSet<IPhysicEvent> events = calculateAvoidMapCrashEvents(moveResult);

      IPhysicEvent passageLine = events.ComeContaints(PhysicEventType.PassageLine) ? events.GetEvent(PhysicEventType.PassageLine) : null;
      IPhysicEvent mapCrash = events.ComeContaints(PhysicEventType.MapCrash) ? events.GetEvent(PhysicEventType.MapCrash) : null;
      IPhysicEvent objectsCrash = events.ComeContaints(PhysicEventType.ObjectsCrash) ? events.GetEvent(PhysicEventType.ObjectsCrash) : null;
      IPhysicEvent crash = (null != objectsCrash && objectsCrash.TickCome > 1) ? objectsCrash : mapCrash;

      if (null != crash) {
        int tickToPassageLine = (null != passageLine) ? passageLine.TickCome : maxIterationCount;

        if (crash.TickCome < tickToPassageLine) {
          Vector dir = new Vector(dirMove.X, dirMove.Y);

          PCar physicCar = crash.CarCome;
          Tuple<Vector, Vector> crashInfo = crash.infoCome as Tuple<Vector, Vector>;
          Logger.instance.Assert(null != crashInfo, "Can't get crash info");
          Vector sideNormal = crashInfo.Item2;

          double angle = dir.Angle.AngleDeviation(sideNormal.Angle);
          if (Vector.sincos(car.Angle).Dot(dir) > 0 || 0 == angle) {
            angle = car.Angle.AngleDeviation(sideNormal.Angle);
          }

          bool notCurrentTurnSide = null != needDirAngle && sideNormal.Dot(needDirAngle) > 0;

          if (!checkStrongParallel(crash) || notCurrentTurnSide) {
            moveResult.WheelTurn = car.WheelTurn - speedSign * Math.Sign(angle) * game.CarWheelTurnChangePerTick;
          }

          bool isParallel = Vector.sincos(car.Angle).Dot(sideNormal).LessDotWithAngle(Math.PI / 9);//20 degrees
          isParallel |= physicCar.Dir.Dot(sideNormal).LessDotWithAngle(Math.PI / 9);//20 degrees

          int ticksToZeroEnginePower = (int)(Math.Abs(car.EnginePower) / game.CarEnginePowerChangePerTick);
          if (!isParallel && speedSign > 0 && crash.TickCome < ticksToZeroEnginePower) {
            if (moveResult.EnginePower > 0.5) {
              moveResult.EnginePower -= game.CarEnginePowerChangePerTick;
            }
          }
          if (!isParallel) {
            moveResult.IsBrake = moveResult.IsBrake || car.Speed() > Constant.MinBrakeSpeed;
          }
        }
      }
    }

    private bool checkStrongParallel(IPhysicEvent mapCrash) {
      if (null == mapCrash) {
        return false;
      }

      Tuple<Vector, Vector> crashInfo = mapCrash.infoCome as Tuple<Vector, Vector>;
      if (null == crashInfo) {
        return false;
      }

      Vector sideNormal = crashInfo.Item2;

      bool isStrongParallel = Vector.sincos(car.Angle).Dot(sideNormal).LessDotWithAngle(Math.PI / 18);//10 degrees
      isStrongParallel &= mapCrash.CarCome.Dir.Dot(sideNormal).LessDotWithAngle(Math.PI / 18);//10 degrees

      return isStrongParallel;
    }

    /// Move to Additional Point
    private void moveToAdditionalPoint(double needAngle) {
      if (null == additionalPoints) {
        return;
      }

      int minTicks = int.MaxValue;
      foreach (Tuple<Vector, double> data in additionalPoints) {
        Vector point = data.Item1;

        IPhysicEvent passageLineEvent = new PassageLineEvent(Vector.sincos(needAngle), point, 0);
        HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
          new MapCrashEvent(),
          passageLineEvent
        };

        if (null != moverSelfMapCrashEvent) {
          pEvents.Add(moverSelfMapCrashEvent.Copy());
        }

        PCar physicCar = new PCar(car, game);
        physicCar.SetupVisualClient(vClient, 0xAAAAFF);
        PhysicEventsCalculator.calculateEvents(physicCar, new MoveToPoint(point, needAngle), pEvents, moveToAddPointEventCheckEnd);

        if (pEvents.ComeContaints(PhysicEventType.MapCrash) || pEvents.ComeContaints(PhysicEventType.ObjectsCrash) || 
           !pEvents.ComeContaints(PhysicEventType.PassageLine)) {
          continue;
        }

        if (passageLineEvent.CarCome.Pos.GetDistanceTo(point) > data.Item2) {
          continue;
        }

        if (passageLineEvent.TickCome < minTicks) {
          needPos = point;
        }
      }
    }

    private bool moveToAddPointEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxIterationCount) {
        return true;
      }

      return pEvents.ComeContaints(PhysicEventType.MapCrash) || 
        pEvents.ComeContaints(PhysicEventType.PassageLine) ||
        pEvents.ComeContaints(PhysicEventType.ObjectsCrash);
    }

    /// Turn MapCrash
    private bool useBrakeForTurn = false;
    private HashSet<IPhysicEvent> calculateTurnMapCrashEvents(PCar iterCar, Vector needDirAngle, bool isBrake) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        passageLineEvent.Copy()
      };

      if (null != moverSelfMapCrashEvent) {
        pEvents.Add(moverSelfMapCrashEvent.Copy());
      }

      PCar physicCar = new PCar(iterCar);
      physicCar.SetupVisualClient(vClient, 0x7F7F44);
      useBrakeForTurn = isBrake;
      physicCar.setBrake(isBrake);
      PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(needDirAngle.Angle), pEvents, calculateTurnMapCrashEventCheckEnd);

      return pEvents;
    }

    private bool calculateTurnMapCrashEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxCheckRotateIterationCount) {
        return true;
      }

      if (useBrakeForTurn) {
        physicCar.setBrake(physicCar.Speed.Length > Constant.MinBrakeSpeed);
      }

      return pEvents.ComeContaints(PhysicEventType.PassageLine);
    }

    /// Turn
    private HashSet<IPhysicEvent> calculateTurnEvents(PCar iterCar, Vector needDirAngle) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        passageLineEvent.Copy(),
        outLineEvent.Copy(),
        angleReachEvent.Copy()
      };

      PCar physicCar = new PCar(iterCar);
      physicCar.SetupVisualClient(vClient, 0x7FFF7F);
      PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(needDirAngle.Angle), pEvents, calculateTurnEventCheckEnd);

      if (!pEvents.Containts(PhysicEventType.SpeedReach)) {
        pEvents.Add(new SpeedReachEvent());
      }

      return pEvents;
    }

    private HashSet<IPhysicEvent> calculateTurnOutLineEvents(PCar iterCar, Vector needDirAngle) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        outLineEvent.Copy()
      };

      PCar physicCar = new PCar(iterCar);
      PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(needDirAngle.Angle), pEvents, calculateTurnEventCheckEnd);

      return pEvents;
    }

    private bool calculateTurnEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxIterationCount) {
        return true;
      }
      return pEvents.ComeContaints(PhysicEventType.OutLine);
    }

    ///Move
    private HashSet<IPhysicEvent> calculateMoveEvents(PCar iterCar) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new MapCrashEvent()
      };

      if (null != moverSelfMapCrashEvent) {
        pEvents.Add(moverSelfMapCrashEvent.Copy());
      }

      PCar physicCar = new PCar(iterCar);
      physicCar.SetupVisualClient(vClient, 0x7FFF7F);
      /*if (null != needPos) {
        PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(Math.Atan2(dirMove.Y, dirMove.X)), pEvents, calculateMoveEventCheckEnd);
      } else {*/
        PhysicEventsCalculator.calculateEvents(physicCar, new MoveToAngleFunction(car.GetAbsoluteAngleTo(defaultPos.X, defaultPos.Y)), pEvents, calculateMoveEventCheckEnd);
      //}

      return pEvents;
    }

    private bool calculateMoveEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxCheckMoveCrashIterationCount) {
        return true;
      }

      return pEvents.ComeContaints(PhysicEventType.MapCrash) || pEvents.ComeContaints(PhysicEventType.ObjectsCrash);
    }

    /// avoid map crash
    private HashSet<IPhysicEvent> calculateAvoidMapCrashEvents(Move currentMove) {
      HashSet<IPhysicEvent> pEvents = new HashSet<IPhysicEvent> {
        new MapCrashEvent()
      };

      if (null != moverSelfMapCrashEvent) {
        pEvents.Add(moverSelfMapCrashEvent.Copy());
      }

      if (null != passageLineEvent) {
        pEvents.Add(passageLineEvent.Copy());
      }

      PCar physicCar = new PCar(car, game);
      physicCar.SetupVisualClient(vClient, 0xFF0000);
      physicCar.setEnginePower(currentMove.EnginePower);
      physicCar.setWheelTurn(0);//currentMove.WheelTurn
      physicCar.setBrake(currentMove.IsBrake);

      PhysicEventsCalculator.calculateEvents(physicCar, new MoveWithOutChange(), pEvents, calculateAvoidSideCrashEventCheckEnd);

      return pEvents;
    }

    private bool calculateAvoidSideCrashEventCheckEnd(PCar physicCar, HashSet<IPhysicEvent> pEvents, int tick) {
      if (tick > maxCheckCrashIterationCount) { 
        return true;
      }

      //physicCar.setBrake(false);

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
  }
}
