﻿using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_StuckOutAction : A_M_BaseMoveAction {
    private const double maxTicks = 80;

    private int ignoreTicks = 10;
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

      if (ignoreTicks > 0) {
        ignoreTicks--;
        zeroSpeedTicks = 0;
        return false;
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

    public override HashSet<ActionType> GetParallelsActions() {
      return new HashSet<ActionType>() {
        ActionType.Shooting
      };
    }

    private bool speedCheck() {
      if (car.Speed2() < 0.05 && Math.Abs(car.EnginePower) > 5 * game.CarEnginePowerChangePerTick) {
        zeroSpeedTicks++;
      } else {
        zeroSpeedTicks = 0;
      }

      if (zeroSpeedTicks > 5) {
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
        ignoreTicks = 10;
        outStuckTicks = 0;
        return;
      }

      double timePower = Math.Sin((Math.PI * 0.5) * (double)(maxTicks - outStuckTicks) / maxTicks);
      move.EnginePower = sign * timePower;

      PointInt dir = path[0].DirOut;

      double angle = sign * car.GetAngleTo(car.X + dir.X, car.Y + dir.Y) * timePower;

      move.WheelTurn = (25 * angle / Math.PI);
    }
  }
}
