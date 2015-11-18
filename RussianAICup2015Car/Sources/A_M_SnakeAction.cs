using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {
  class A_M_SnakeAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.wayCells.Length, "incorrect way cells count.");

      PointInt posIn = path.wayCells[1].Pos;
      PointInt posOut = path.wayCells[2].Pos;

      PointInt dirIn = path.wayCells[1].DirIn;
      PointInt dirOut = path.wayCells[2].DirOut;

      if (null == dirOut || dirOut.Equals(new PointInt(0))) {
        return false;
      }

      PointInt dir = new PointInt(posOut.X - posIn.X, posOut.Y - posIn.Y);

      return dirIn.Equals(dirOut) && (dir.Equals(dirIn.Perpendicular()) || dir.Equals(dirIn.Perpendicular().Negative()));
    }

    public override void execute(Move move) {
      move.EnginePower = 1.0;
    }

    public override HashSet<ActionType> blockers { get { return new HashSet<ActionType>() { ActionType.InitialFreeze, ActionType.StuckOut }; } }
  }
}
