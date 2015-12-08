using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class StuckOutMoving : MovingBase {
    private const double maxTicks = 80;

    private int zeroSpeedTicks = 0;
    private int outStuckTicks = 0;
    private int sign = -1;

    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      if (outStuckTicks > 0) {
        if (15 < outStuckTicks && outStuckTicks < maxTicks * 0.5 && speedCheck()) {
          sign *= -1;
        }
        return true;
      }

      if (speedCheck()) {
        sign = -1;
        return true;
      }

      return false;
    }

    public override void execute(Move move) {
      runOut(move);
    }

    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.Shooting
      };
    }

    private bool speedCheck() {
      if (car.Speed2() < 0.1) {
        zeroSpeedTicks++;
      } else {
        zeroSpeedTicks = 0;
      }

      if (zeroSpeedTicks > 10) {
        zeroSpeedTicks = 0;
        outStuckTicks = 1;
        return true;
      }

      return false;
    }

    private void runOut(Move move) {
      if (sign * car.EnginePower > 0 || outStuckTicks > 1) {
        outStuckTicks++;
      }

      if (outStuckTicks > maxTicks) {
        outStuckTicks = 0;
        return;
      }

      double timePower = Math.Sin((Math.PI * 0.5) * (double)(maxTicks - outStuckTicks) / maxTicks);
      timePower = 1.2 * timePower - 0.2;
      move.EnginePower = sign * timePower;
      if (timePower < 1.0e-3) {
        move.IsBrake = true;
      }

      TileDir dir = path[0].DirOut;

      double angle = sign * car.GetAngleTo(car.X + dir.X, car.Y + dir.Y) * timePower;
      move.WheelTurn = (25 * angle / Math.PI);
    }
  }
}
