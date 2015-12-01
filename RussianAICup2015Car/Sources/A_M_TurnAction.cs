using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_TurnAction : A_M_BaseMoveAction {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      PointInt dirIn = path[1].DirIn;
      PointInt dirOut = path[1].DirOut;

      return dirIn.Equals(dirOut.Perpendicular()) || dirIn.Equals(dirOut.Perpendicular().Negative()); 
    }

    public override void execute(Move move) {
      PointInt dirMove = path[0].DirOut;
      PointInt dirEnd = path[1].DirOut;

      Vector centerTurnPos = GetWayEnd(path[1].Pos, new PointInt(0));
      Vector endPos = GetWayEnd(path[1].Pos, dirEnd);

      PhysicMoveCalculator calculator = new PhysicMoveCalculator();
      calculator.setupEnvironment(car, game);

      Move needMove = calculator.calculateMove(centerTurnPos, endPos, new Vector(dirMove.X, dirMove.Y), new Vector(dirEnd.X, dirEnd.Y));
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
