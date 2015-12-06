using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class PreTurnMoving : MovingBase {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      if (!path[1].DirIn.Equals(path[1].DirOut)) {
        return false;
      }

      TileDir dirIn = path[2].DirIn;
      TileDir dirOut = path[2].DirOut;

      if (null == dirOut) {
        return false;
      }

      return dirIn.Equals(dirOut.PerpendicularLeft()) || dirIn.Equals(dirOut.PerpendicularRight()); 
    }

    public override void execute(Move move) {
      TileDir dirMove = path[0].DirOut;
      TileDir dirEnd = path[2].DirOut;

      Vector endPos = GetWayEnd(path[2].Pos, new TileDir(0));

      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);

      Move needMove = calculator.calculateMove(endPos, dirMove, new Vector(dirEnd.X + dirMove.X, dirEnd.Y + dirMove.Y));
      move.IsBrake = needMove.IsBrake;
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }
  }
}
