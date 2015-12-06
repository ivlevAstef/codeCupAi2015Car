using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class TurnMoving : MovingBase {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      TileDir dirIn = path[1].DirIn;
      TileDir dirOut = path[1].DirOut;

      return dirIn.Equals(dirOut.Perpendicular()) || dirIn.Equals(dirOut.Perpendicular().Negative()); 
    }

    public override void execute(Move move) {
      TileDir dirMove = path[0].DirOut;
      TileDir dirEnd = path[1].DirOut;

      Vector endPos = GetWayEnd(path[1].Pos, dirEnd);

      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);

      Move needMove = calculator.calculateMove(endPos, dirMove, new Vector(dirEnd.X, dirEnd.Y));
      move.IsBrake = needMove.IsBrake;
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }

    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.Shooting,
        ActionType.OilSpill,
      };
    }
  }
}
