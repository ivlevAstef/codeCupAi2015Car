using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public interface IPhysicMoveFunction {
    void Iteration(PCar car, int iterationCount);
  }

  public class MoveToAngleFunction : IPhysicMoveFunction {
    public static World world = null;
    public static double RotationFrictionFactor = 0;
    public static double WheelTurnChangePerTick = 0;

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

        PCar zeroWheelTurn = physicCarForZeroWheelTurn(car);
        double angleDeviation = angle.AngleDeviation(zeroWheelTurn.Angle);

        if (Math.Abs(angleDeviation) < RotationFrictionFactor) {
          car.setWheelTurn(0);
        } else {
          car.setWheelTurn(Math.Sign(angleDeviation));
        }

        car.Iteration(1);
      }
    }

    private PCar physicCarForZeroWheelTurn(PCar car) {
      PCar physicCar = new PCar(car);
      int ticks = (int)Math.Abs(Math.Round(physicCar.WheelTurn / WheelTurnChangePerTick));

      physicCar.setWheelTurn(0);
      physicCar.Iteration(ticks);

      return physicCar;
    }
  }
}
