using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  public class PhysicMoveCalculator {
    private const int maxIterationCount = 150;
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

    public Move calculateMove(Vector idealPos, Vector dirMove, Vector idealDir) {
      Dictionary<MovedEvent, PhysicCar> events = calculateEvents(idealPos, dirMove, idealDir);

      Move result = new Move();
      result.EnginePower = 1.0;

      double idealAngle = Math.Atan2(idealDir.Y, idealDir.X);
      double angleDeviation = idealAngle.AngleDeviation(car.Angle);
      Vector dirMovePerpendicular = dirMove.Perpendicular() * Math.Sign(dirMove.Perpendicular().Dot(idealDir));

      if (events.ContainsKey(MovedEvent.PassageLine)) {
        Vector pos = events[MovedEvent.PassageLine].Pos;
        Vector posSpeedReach = events.ContainsKey(MovedEvent.SpeedReach) ? events[MovedEvent.SpeedReach].Pos : null;

        if (null == posSpeedReach || (posSpeedReach - pos).Dot(dirMove) > oneLineWidth) {
          result.IsBrake = true;
        }

        result.WheelTurn = car.WheelTurn + Math.Sign(angleDeviation) * game.CarWheelTurnChangePerTick;
      }

      if (events.ContainsKey(MovedEvent.SideCrash)) {
        result.EnginePower = car.EnginePower - game.CarEnginePowerChangePerTick;
        result.WheelTurn = car.WheelTurn - Math.Sign(angleDeviation) * game.CarWheelTurnChangePerTick;
      }

      return result;
    }

    private Dictionary<MovedEvent, PhysicCar> calculateEvents(Vector idealPos, Vector dirMove, Vector idealDir) {
      Dictionary<MovedEvent, PhysicCar> result = new Dictionary<MovedEvent, PhysicCar>();

      Vector center = car.CenterTile(game);
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
        if (!result.ContainsKey(MovedEvent.SideCrash) && checkSideCrash(physicCar,idealAngle, center, dirMove, idealDir)) {
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

    private bool checkSideCrash(PhysicCar car, double idealAngle, Vector center, Vector dirMove, Vector idealDir) {
      double distanceToSide = game.TrackTileSize * 0.5 - (game.TrackTileMargin + game.CarHeight * 0.55);
      double distanceToEndSide = game.TrackTileSize * 0.5 + (game.TrackTileMargin + game.CarHeight * 0.55);

      Vector distanceFromCenter = car.Pos - center;
      Vector dirMovePerpendicular = dirMove.Perpendicular() * Math.Sign(dirMove.Perpendicular().Dot(idealDir));

      double crossLength = distanceFromCenter.Dot(dirMovePerpendicular);

      if (Math.Abs(crossLength) < distanceToSide) {
        return false;
      }

      if (1 == Math.Sign(crossLength)) { //Side with turn
        return distanceFromCenter.Dot(dirMove) < distanceToEndSide;//not turn
      }

      return true;
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
