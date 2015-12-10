using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public interface IPhysicMoveFunction {
    void Iteration(PCar car, int iterationCount);
  }

  public class MoveToAngleFunction : IPhysicMoveFunction {
    private static World world = null;
    private static double rotationFrictionFactor = 0;

    public static void setupEnvironment(World lWorld, Game game) {
      world = lWorld;
      rotationFrictionFactor = game.CarRotationFrictionFactor;
    }

    private double angle;
    private IntersectOilStickEvent intersecOildStickEvent;

    public MoveToAngleFunction(double angle) {
      this.angle = angle;
      intersecOildStickEvent = new IntersectOilStickEvent(world);
    }

    public void Iteration(PCar car, int iterationCount) {
      for (int i = 0; i < iterationCount; i++) {
        if (intersecOildStickEvent.Check(car)) {
          car.traveledOnOil(intersecOildStickEvent.InfoForCheck as OilSlick);
        }

        PCar zeroWheelTurn = car.GetZeroWheelTurnCar();
        double angleDeviation = angle.AngleDeviation(zeroWheelTurn.Angle);

        if (Math.Abs(angleDeviation) < rotationFrictionFactor) {
          car.setWheelTurn(0);
        } else {
          car.setWheelTurn(Math.Sign(angleDeviation));
        }

        car.Iteration(1);
      }
    }
  }

  public class MoveToPoint : IPhysicMoveFunction {
    private static double rotationFrictionFactor = 0;

    public static void setupEnvironment(World lWorld, Game game) {
      rotationFrictionFactor = game.CarRotationFrictionFactor;
    }

    private Vector point;

    public MoveToPoint(Vector point) {
      this.point = point;
    }

    public void Iteration(PCar car, int iterationCount) {
      for (int i = 0; i < iterationCount; i++) {
        double angle = (point - car.Pos).Angle;

        PCar zeroWheelTurn = car.GetZeroWheelTurnCar();
        double angleDeviation = angle.AngleDeviation(zeroWheelTurn.Angle);

        if (Math.Abs(angleDeviation) < rotationFrictionFactor) {
          car.setWheelTurn(0);
        } else {
          car.setWheelTurn(Math.Sign(angleDeviation));
        }

        car.Iteration(1);
      }
    }
  }

  public class MoveWithOutChange : IPhysicMoveFunction {
    public MoveWithOutChange() {
    }

    public void Iteration(PCar car, int iterationCount) {
      car.Iteration(iterationCount);
    }
  }
}
