using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;

namespace RussianAICup2015Car.Sources {
  class A_M_ForwardAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.wayCells.Length, "incorrect way cells count.");

      foreach (PathCell cell in path.wayCells) {
        if (null == cell.DirOut || !cell.DirOut.Equals(cell.DirIn)) {
          return false;
        }
      }

      return true;
    }

    public override void execute(Dictionary<ActionType, bool> valid, Move move) {

    }
  }
}
