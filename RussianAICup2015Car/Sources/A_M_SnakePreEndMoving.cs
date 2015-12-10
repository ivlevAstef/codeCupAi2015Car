using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class SnakePreEndMoving : MovingBase {
    private const int offset = 1;

    public override bool valid() {
      return PathCheckResult.No == checkSnakeWithOffset(offset);
    }

    public override void execute(Move move) {
      TileDir dirMove = path[0 + offset].DirOut;
      TileDir dirEnd = path[1 + offset].DirOut;

      Vector endPos = GetWayEnd(path[1 + offset].Pos, dirEnd);
      dirMove = path[0].DirOut;

      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);

      Vector dir = new Vector(dirEnd.X, dirEnd.Y);
      Move needMove = calculator.calculateMove(endPos, dirMove, dir, 1.0);
      move.IsBrake = needMove.IsBrake;
      //move.EnginePower = needMove.EnginePower;
      //move.WheelTurn = needMove.WheelTurn;
    }
  }
}
