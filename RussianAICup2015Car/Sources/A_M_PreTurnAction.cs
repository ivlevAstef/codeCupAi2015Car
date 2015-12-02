using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_PreTurnAction : A_M_BaseMoveAction {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      if (!path[1].DirIn.Equals(path[1].DirOut)) {
        return false;
      }

      PointInt dirIn = path[2].DirIn;
      PointInt dirOut = path[2].DirOut;

      if (null == dirOut) {
        return false;
      }

      return dirIn.Equals(dirOut.PerpendicularLeft()) || dirIn.Equals(dirOut.PerpendicularRight()); 
    }

    public override void execute(Move move) {
      PointInt dirMove = path[0].DirOut;
      PointInt dirEnd = path[2].DirOut;

      Vector endPos = GetWayEnd(path[2].Pos, new PointInt(0));

      PhysicMoveCalculator calculator = new PhysicMoveCalculator();
      calculator.setupEnvironment(car, map, game);

      Move needMove = calculator.calculateMove(endPos, new Vector(dirMove.X, dirMove.Y), new Vector(dirEnd.X + dirMove.X, dirEnd.Y + dirMove.Y));
      move.IsBrake = needMove.IsBrake;
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }
  }
}
