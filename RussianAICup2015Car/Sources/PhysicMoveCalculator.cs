using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  public class PhysicMoveCalculator {
    private const int maxIterationCount = 250;
    private double oneLineWidth;

    private enum MovedEvent {
      PassageLine,
      AngleReach,
      SpeedReach,
      SideCrash,
    };

    private Car car;
    private Game game;

    public void setupEnvironment(Car car, Game game) {
      this.car = car;
      this.game = game;
      oneLineWidth = (game.TrackTileSize - 2 * game.TrackTileMargin) / 4;
    }

    public Move calculateMove(Vector idealPos, Vector dirMove, Vector idealDir, double lineCount = 1.25) {
      Dictionary<MovedEvent, Tuple<PhysicCar, int>> events = calculateEvents(idealPos, dirMove, idealDir);

      Move result = new Move();
      result.EnginePower = 1.0;
      result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, Math.Atan2(dirMove.Y, dirMove.X));

      if (events.ContainsKey(MovedEvent.PassageLine)) {
        Vector posSpeedReach = events.ContainsKey(MovedEvent.SpeedReach) ? events[MovedEvent.SpeedReach].Item1.Pos : null;

        if (null == posSpeedReach || (posSpeedReach - idealPos).Dot(dirMove) > lineCount * oneLineWidth) {
          result.IsBrake = car.Speed() > 9;
        }

        int tickToPassageLine = events[MovedEvent.PassageLine].Item2;
        int tickToAngleReach = events.ContainsKey(MovedEvent.SpeedReach) ? events[MovedEvent.SpeedReach].Item2 : maxIterationCount;
        bool magnifiedPos = tickToAngleReach < tickToPassageLine;

        double idealAngle = magnifiedPos ? car.GetAbsoluteAngleTo(idealPos.X, idealPos.Y) : Math.Atan2(idealDir.Y, idealDir.X);
        result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, idealAngle);
      }

      if (events.ContainsKey(MovedEvent.SideCrash)) {
        Vector pos = events[MovedEvent.SideCrash].Item1.Pos;
        Vector sideDir = (pos - new Vector(car.X, car.Y));
        Vector improvedSideDir = (pos + dirMove - new Vector(car.X, car.Y));
        double angle = improvedSideDir.Angle.AngleDeviation(sideDir.Angle);

        bool nearSide = Math.Abs(sideDir.Dot(dirMove.Perpendicular())) < 10;
        if (!nearSide) {
          result.WheelTurn = car.WheelTurn + Math.Sign(angle) * game.CarWheelTurnChangePerTick;
        }

        if (events[MovedEvent.SideCrash].Item2 < 10) {
          result.IsBrake = car.Speed() > 9;
        }
      }

      return result;
    }

    private Dictionary<MovedEvent, Tuple<PhysicCar, int>> calculateEvents(Vector idealPos, Vector dirMove, Vector idealDir) {
      Dictionary<MovedEvent, Tuple<PhysicCar, int>> result = new Dictionary<MovedEvent, Tuple<PhysicCar, int>>();

      double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);
      PhysicCar physicCar = new PhysicCar(car, game);
      physicCar.setEnginePower(1.0);

      for (int i = 0; i < maxIterationCount; i++) {
        bool passageLine = checkPassageLine(physicCar, idealPos, dirMove);
        bool angleReach = checkAngleReach(physicCar, idealAngle);
        bool speedReach = checkSpeedReach(physicCar);
        bool sideCrash = checkSideCrash(physicCar, dirMove, idealDir);

        if (!result.ContainsKey(MovedEvent.PassageLine) && passageLine) {
          result[MovedEvent.PassageLine] = new Tuple<PhysicCar,int>(new PhysicCar(physicCar), i);
        }
        if (!result.ContainsKey(MovedEvent.AngleReach) && angleReach) {
          result[MovedEvent.AngleReach] = new Tuple<PhysicCar,int>(new PhysicCar(physicCar), i);
        }
        if (!result.ContainsKey(MovedEvent.SpeedReach) && angleReach && speedReach) {
          result[MovedEvent.SpeedReach] = new Tuple<PhysicCar,int>(new PhysicCar(physicCar), i);
        }
        if (!result.ContainsKey(MovedEvent.PassageLine) && !result.ContainsKey(MovedEvent.SideCrash) && sideCrash) {
          result[MovedEvent.SideCrash] = new Tuple<PhysicCar, int>(new PhysicCar(physicCar), i);
        }

        if (result.ContainsKey(MovedEvent.PassageLine) && result.ContainsKey(MovedEvent.SpeedReach)) {
          break;
        }

        //move
        PhysicCar zeroWheelTurn = PhysicCarForZeroWheelTurn(physicCar, game);
        double angleDeviation = idealAngle.AngleDeviation(zeroWheelTurn.Angle);

        if (Math.Abs(angleDeviation) < game.CarRotationFrictionFactor) {
          physicCar.setWheelTurn(0);
        } else {
          physicCar.setWheelTurn(Math.Sign(angleDeviation));
        }

        physicCar.Iteration(1);
      }

      return result;
    }

    private bool checkPassageLine(PhysicCar car, Vector idealPos, Vector dirMove) {
      return Math.Sign((car.Pos - idealPos).Dot(dirMove)) != Math.Sign((car.LastPos - idealPos).Dot(dirMove));
    }

    private bool checkAngleReach(PhysicCar car, double idealAngle) {
      double angleDeviation = idealAngle.AngleDeviation(car.Angle);
      
      return Math.Abs(angleDeviation) < game.CarRotationFrictionFactor &&
             Math.Abs(car.WheelTurn)  < game.CarWheelTurnChangePerTick;
    }

    private bool checkSpeedReach(PhysicCar car) {
      return (car.Speed.Normalize() - car.Dir).Length < 1.0e-3;
    }

    private bool checkSideCrash(PhysicCar car, Vector dirMove, Vector idealDir) {
      return CollisionDetector.instance.IntersectCarWithMap(car.Pos, car.Dir);
    }

    private static double WheelTurnForEndZeroWheelTurn(Car car, Game game, double finalAngle) {
      PhysicCar physicCar = new PhysicCar(car, game);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / game.CarWheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);

      double angleDeviation = finalAngle.AngleDeviation(physicCar.Angle);

      if (Math.Abs(angleDeviation) < game.CarRotationFrictionFactor) {
        return 0;
      }

      return car.WheelTurn + game.CarWheelTurnChangePerTick * Math.Sign(angleDeviation);
    }

    private static PhysicCar PhysicCarForZeroWheelTurn(PhysicCar car, Game game) {
      PhysicCar physicCar = new PhysicCar(car);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / game.CarWheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);

      return physicCar;
    }
  }
}
