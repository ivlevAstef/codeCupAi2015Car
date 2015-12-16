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

    public static void setupEnvironment(World lWorld) {
      world = lWorld;
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

        double speedSign = Math.Sign(car.Dir.Dot(car.Speed));
        double wheelTurn = car.WheelTurnForEndZeroWheelTurn(angle, speedSign);
        car.setWheelTurn(wheelTurn);

        car.Iteration(1);
      }
    }
  }

  public class MoveToPoint : IPhysicMoveFunction {
    private static World world = null;

    public static void setupEnvironment(World lWorld) {
      world = lWorld;
    }

    private Vector point;
    private IntersectOilStickEvent intersecOildStickEvent;

    public MoveToPoint(Vector point) {
      this.point = point;
      intersecOildStickEvent = new IntersectOilStickEvent(world);
    }

    public void Iteration(PCar car, int iterationCount) {
      for (int i = 0; i < iterationCount; i++) {
        if (intersecOildStickEvent.Check(car)) {
          car.traveledOnOil(intersecOildStickEvent.InfoForCheck as OilSlick);
        }

        double angle = (point - car.Pos).Angle;

        double speedSign = Math.Sign(car.Dir.Dot(car.Speed));
        double wheelTurn = car.WheelTurnForEndZeroWheelTurn(angle, speedSign);
        car.setWheelTurn(wheelTurn);

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
