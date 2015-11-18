using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {
  class A_M_AroundAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.wayCells.Length, "incorrect way cells count.");

      PointInt posIn = path.wayCells[1].Pos;
      PointInt posOut = path.wayCells[2].Pos;

      PointInt dirIn = path.wayCells[1].DirIn;
      PointInt dirOut = path.wayCells[2].DirOut;

      if (null == dirOut) {
        return false;
      }

      return dirIn.Equals(dirOut.Negative()) && !posIn.Equals(posOut);
    }

    public override void execute(Dictionary<ActionType, bool> valid, Move move) {

    }
  }
}
