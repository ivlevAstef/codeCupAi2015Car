using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System;

namespace RussianAICup2015Car.Sources {
  public static class PhysicExtensions {
    public static Move moveTo(this Car car, Game game, Vector pos, Vector dirMove, Vector dir) {
      const int maxIterations = 100;
      double finalAngle = Math.Atan2(dir.Y, dir.X);

      PhysicCar physicCar = new PhysicCar(car, game);
      physicCar.setEnginePower(1.0);

      for (int i = 0; i < maxIterations; i++) {
        PhysicCar zeroWheelTurn = physicCar.PhysicCarForZeroWheelTurn(game);
        double angleDeviation = finalAngle.AngleDeviation(zeroWheelTurn.Angle);
        bool angleReach = false;

        if (Math.Abs(angleDeviation) < game.CarRotationFrictionFactor) {
          physicCar.setWheelTurn(0);
          angleReach = Math.Abs(physicCar.WheelTurn) < game.CarWheelTurnChangePerTick;
        } else {
          physicCar.setWheelTurn(Math.Sign(angleDeviation));
        }

        bool speedReach = (physicCar.Speed.Normalize() - physicCar.Dir).Length < 1.0e-3;

        if (angleReach && speedReach) {
          break;
        }

        physicCar.Iteration(1);
      }

      return car.moveFor(physicCar, game, pos, dirMove, dir);

    }

    public static Move moveFor(this Car car, PhysicCar finalCar, Game game, Vector pos, Vector dirMove, Vector dir) {
      Move result = new Move();
      result.EnginePower = 1.0;

      double SideSize = game.TrackTileMargin + game.CarHeight * 0.55;
      double OneLine = (game.TrackTileSize - 2 * game.TrackTileMargin) / 4;

      Vector distance = pos - finalCar.Pos;
      double finalAngle = Math.Atan2(dir.Y, dir.X);

      if (Math.Abs(distance.Dot(dirMove)) < 2 * OneLine) {//ideal
        result.WheelTurn = car.WheelTurnForEndZeroWheelTurn(game, finalAngle);
      } else if (distance.Dot(dirMove) < 0 && car.Speed() > 8) {//overmove
        result.IsBrake = true;
      }

      Vector distanceFromCenter = car.CenterTile(game) - finalCar.Pos;
      double distanceFromCenterLength = distance.Dot(dirMove.Perpendicular());

      if (Math.Abs(distanceFromCenterLength) > game.TrackTileSize*0.5 - SideSize) {//side crash
        result.EnginePower = car.EnginePower - game.CarEnginePowerChangePerTick;
        double sign = Math.Sign(distanceFromCenterLength * dirMove.Perpendicular().Dot(dir));
        sign *= Math.Sign(finalAngle.AngleDeviation(car.Angle));
        result.WheelTurn = car.WheelTurn - sign * game.CarWheelTurnChangePerTick;
      }

      return result;
    }

    public static double WheelTurnForEndZeroWheelTurn(this Car car, Game game, double finalAngle) {
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

    public static PhysicCar PhysicCarForZeroWheelTurn(this PhysicCar car, Game game) {
      PhysicCar physicCar = new PhysicCar(car);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / game.CarWheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);

      return physicCar;
    }

  }
}
