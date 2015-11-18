using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_StuckOutAction : A_BaseAction {
    private int ignoreTicks = 0;
    private int zeroSpeedTicks = 0;
    private int outStuckTicks = 0;

    public override bool valid() {
      if (outStuckTicks > 0) {
        return true;
      }

      if (0 == car.Durability) {
        ignoreTicks = 15;
        return false;
      }

      if (ignoreTicks > 0) {
        ignoreTicks--;
        zeroSpeedTicks = 0;
        return false;
      }

      double speed = car.SpeedX * car.SpeedX + car.SpeedY * car.SpeedY;
      if (speed < 0.05) {
        zeroSpeedTicks++;
      } else {
        zeroSpeedTicks = 0;
      }

      if (zeroSpeedTicks > 5) {
        outStuckTicks = 1;
        return true;
      }

      return false;
    }

    public override void execute(Move move) {
      runOut(move);
    }

    public override void blockedBy(HashSet<ActionType> actions) {
      ignoreTicks = 10;
      zeroSpeedTicks = 0;
      outStuckTicks = 0;
    }

    public override HashSet<ActionType> blockers { get { return new HashSet<ActionType>() { ActionType.InitialFreeze }; } }

    private void runOut(Move move) {
      const double maxTicks = 115;

      if (car.EnginePower < 0 || outStuckTicks > 1) {
        outStuckTicks++;
      }

      if (outStuckTicks > maxTicks) {
        ignoreTicks = 10;
        outStuckTicks = 0;
        return;
      }

      double timePower = Math.Sin((Math.PI * 0.5) * (double)(maxTicks - outStuckTicks) / maxTicks);
      move.EnginePower = -timePower;

      PointInt dir = path.wayCells[0].DirOut;

      double angle = -car.GetAngleTo(car.X + dir.X, car.Y + dir.Y) * timePower;

      move.WheelTurn = (25 * angle / Math.PI);
    }
  }
}
