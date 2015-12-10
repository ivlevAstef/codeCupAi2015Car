﻿using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class AroundMoving : MovingBase {
    private int offset = 1;

    public override bool valid() {
      for (offset = 0; offset <= 1; offset++) {
        if (PathCheckResult.Yes == checkAround(offset)) {
          return true;
        }
      }

      return false;
    }

    public override void execute(Move move) {
      TileDir dirMove = path[offset].DirOut;
      TileDir dirEnd = path[1 + offset].DirOut;

      Vector endPos = GetWayEnd(path[1 + offset].Pos, dirMove.Negative(), 1.0);

      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);

      Vector dir = new Vector(dirEnd.X - dirMove.X, dirEnd.Y - dirMove.Y).Normalize();
      Move needMove = calculator.calculateTurn(endPos, dirMove, dir);

      move.IsBrake = needMove.IsBrake;
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }

    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.Shooting,
        ActionType.OilSpill
      };
    }
  }
}
