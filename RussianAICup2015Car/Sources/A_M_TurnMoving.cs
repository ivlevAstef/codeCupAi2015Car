using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class TurnMoving : MovingBase {
    private int offset = 0;

    public override bool valid() {
      for (offset = 0; offset <= 1; offset++) {
        if (PathCheckResult.Yes == checkTurn(offset)) {
          return true;
        }
      }

      return false;
    }

    public override void execute(Move move) {
      TileDir dirMove = path[offset].DirOut;
      TileDir dirEnd = path[1 + offset].DirOut;

      Vector endPos = GetWayEnd(path[1 + offset].Pos, TileDir.Zero);

      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);

      Move needMove = calculator.calculateMove(endPos, dirMove, new Vector(dirEnd.X, dirEnd.Y), 1.0);
      move.IsBrake = needMove.IsBrake;
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }

    public override List<ActionType> GetParallelsActions() {
      List<ActionType> result = new List<ActionType>() {
        ActionType.Shooting,
        ActionType.OilSpill,
      };

      if (0 != offset) {
        result.Add(ActionType.MoveToBonus);
      }

      return result;
    }
  }
}
