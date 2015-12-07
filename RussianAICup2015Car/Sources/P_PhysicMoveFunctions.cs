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


  public class MoveToTile : IPhysicMoveFunction {
    private TilePos tilePos;

    public MoveToTile(TilePos pos) {
      this.tilePos = pos;
    }

    public void Iteration(PCar car, int iterationCount) {
      Vector position = tilePos.ToVector(0.5, 0.5);

      for (int i = 0; i < iterationCount; i++) {
        Vector dir = position - car.Pos;

        PCar zeroWheelTurn = car.GetZeroWheelTurnCar();
        double angleDeviation = dir.Angle.AngleDeviation(zeroWheelTurn.Angle);

        car.setWheelTurn(Math.Sign(angleDeviation));
        car.setBrake(car.Speed.Length > Constant.MinBrakeSpeed);
        car.Iteration(2);
      }
    }
  }
}
