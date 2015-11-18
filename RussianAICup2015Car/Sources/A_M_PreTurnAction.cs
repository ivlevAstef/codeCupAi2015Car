using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_PreTurnAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.wayCells.Length, "incorrect way cells count.");

      if (!path.wayCells[0].DirIn.Equals(path.wayCells[0].DirOut)) {
        return false;
      }
      if (!path.wayCells[1].DirIn.Equals(path.wayCells[1].DirOut)) {
        return false;
      }

      PointInt dirIn = path.wayCells[2].DirIn;
      PointInt dirOut = path.wayCells[2].DirOut;

      if (null == dirOut) {
        return true;
      }

      return dirIn.Equals(dirOut.Perpendicular()) || dirIn.Equals(dirOut.Perpendicular().Negative()); 
    }

    public override void execute(Move move) {
      move.EnginePower = 1.0;
    }

    public override HashSet<ActionType> blockers { get { return new HashSet<ActionType>() { ActionType.InitialFreeze, ActionType.StuckOut }; } }
  }
}
