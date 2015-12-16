using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using RussianAICup2015Car.Sources.Physic;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class SnakePreEndMoving : MovingBase {
    private const int offset = 1;

    public override bool valid() {
      return PathCheckResult.No == checkSnakeWithOffset(offset);
    }

    public override void execute(Move move) {
      TileDir dirMove = path[offset].DirOut;
      TileDir dirEnd = path[1 + offset].DirOut;

      MovingCalculator calculator = new MovingCalculator();
      calculator.setupEnvironment(car, game, world);
      calculator.setupMapInfo(dirMove, path[0].Pos, path[1 + offset].Pos);
      calculator.setupDefaultAction(GetWayEnd(path[1 + offset].Pos, TileDir.Zero));

      Vector endDir = new Vector(dirEnd.X, dirEnd.Y);

      calculator.setupAngleReach(endDir);
      calculator.setupPassageLine(GetWayEnd(path[1 + offset].Pos, dirEnd), new Vector(dirMove.X, dirMove.Y), 1.25);
      calculator.setupAdditionalPoints(this.additionalPoints);

      Move needMove = calculator.calculateTurn(endDir);
      if (needMove.IsBrake) {
        move.IsBrake = true;
      }
      //move.EnginePower = needMove.EnginePower;
      //move.WheelTurn = needMove.WheelTurn;
    }
  }
}
