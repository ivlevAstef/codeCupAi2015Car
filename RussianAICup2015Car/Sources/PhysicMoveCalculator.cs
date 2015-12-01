using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  public class PhysicMoveCalculator {
    private const int maxIterationCount = 250;
    private double oneLineWidth;

    private enum MovedEvent {
      PassagePos,
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

    public Move calculateMove(Vector turnPos, Vector idealPos, Vector dirMove, Vector idealDir) {
      Dictionary<MovedEvent, PhysicCar> events = calculateEvents(turnPos, idealPos, dirMove, idealDir);

      Move result = new Move();
      result.EnginePower = 1.0;
      result.WheelTurn = WheelTurnForEndZeroWheelTurn(car, game, Math.Atan2(dirMove.Y, dirMove.X));

      double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);
      double angleDeviation = idealAngle.AngleDeviation(car.Angle);
      Vector dirMovePerpendicular = dirMove.Perpendicular() * Math.Sign(dirMove.Perpendicular().Dot(idealDir));

      if (events.ContainsKey(MovedEvent.PassageLine)) {
        Vector posSpeedReach = events.ContainsKey(MovedEvent.SpeedReach) ? events[MovedEvent.SpeedReach].Pos : null;

        if (null == posSpeedReach || (posSpeedReach - idealPos).Dot(dirMove) > 1.25 * oneLineWidth) {
          result.IsBrake = true;
        }

        result.WheelTurn = car.WheelTurn + Math.Sign(angleDeviation) * game.CarWheelTurnChangePerTick;
      }

      if (events.ContainsKey(MovedEvent.SideCrash)) {
        Vector pos = events[MovedEvent.SideCrash].Pos;
        double sideDistance = (pos - new Vector(car.X, car.Y)).Dot(dirMovePerpendicular);

        result.WheelTurn = car.WheelTurn - Math.Sign(angleDeviation * sideDistance) * game.CarWheelTurnChangePerTick;
      }

      return result;
    }

    private Dictionary<MovedEvent, PhysicCar> calculateEvents(Vector turnPos, Vector idealPos, Vector dirMove, Vector idealDir) {
      Dictionary<MovedEvent, PhysicCar> result = new Dictionary<MovedEvent, PhysicCar>();

      double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);
      PhysicCar physicCar = new PhysicCar(car, game);
      physicCar.setEnginePower(1.0);

      for (int i = 0; i < maxIterationCount; i++) {
        if (!result.ContainsKey(MovedEvent.PassagePos) && checkPassagePos(physicCar, idealPos)) {
          result[MovedEvent.PassagePos] = new PhysicCar(physicCar);
        }
        if (!result.ContainsKey(MovedEvent.PassageLine) && checkPassageLine(physicCar, idealPos, dirMove)) {
          result[MovedEvent.PassageLine] = new PhysicCar(physicCar);
        }
        if (!result.ContainsKey(MovedEvent.AngleReach) && checkAngleReach(physicCar, idealAngle)) {
          result[MovedEvent.AngleReach] = new PhysicCar(physicCar);
        }
        if (!result.ContainsKey(MovedEvent.SpeedReach) && checkAngleReach(physicCar, idealAngle) && checkSpeedReach(physicCar)) {
          result[MovedEvent.SpeedReach] = new PhysicCar(physicCar);
        }
        if (!result.ContainsKey(MovedEvent.SideCrash) && checkSideCrash(physicCar, turnPos, dirMove, idealDir)) {
          result[MovedEvent.SideCrash] = new PhysicCar(physicCar);
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

    private bool checkPassagePos(PhysicCar car, Vector idealPos) {
      return (car.Pos - idealPos).Length < oneLineWidth* 0.5;
    }

    private bool checkPassageLine(PhysicCar car, Vector idealPos, Vector dirMove) {
      return Math.Abs((car.Pos - idealPos).Dot(dirMove)) < oneLineWidth * 0.25;
    }

    private bool checkAngleReach(PhysicCar car, double idealAngle) {
      double angleDeviation = idealAngle.AngleDeviation(car.Angle);
      
      return Math.Abs(angleDeviation) < game.CarRotationFrictionFactor &&
             Math.Abs(car.WheelTurn)  < game.CarWheelTurnChangePerTick;
    }

    private bool checkSpeedReach(PhysicCar car) {
      return (car.Speed.Normalize() - car.Dir).Length < 1.0e-3;
    }

    private bool checkSideCrash(PhysicCar car, Vector turnPos, Vector dirMove, Vector idealDir) {
      double distanceToSide = game.TrackTileSize * 0.5 - (game.TrackTileMargin + game.CarHeight * 0.6);

      Vector distanceFromCenter = car.Pos - turnPos;
      Vector dirMovePerpendicular = dirMove.Perpendicular() * Math.Sign(dirMove.Perpendicular().Dot(idealDir));

      double crossLength = distanceFromCenter.Dot(dirMovePerpendicular);

      if (Math.Abs(crossLength) < distanceToSide) {
        return false;
      }

      if (1 == Math.Sign(crossLength)) { //Side with turn
        return distanceFromCenter.Dot(dirMove) < -distanceToSide;//not turn
      }

      return true;
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
