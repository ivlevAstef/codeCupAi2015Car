using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_BackwardAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 == path.wayCells.Length, "incorrect way cells count.");

      PointInt dir = path.wayCells[0].DirIn;

      if (!dir.Equals(path.wayCells[0].DirOut)) {
        return false;
      }

      double angle = car.GetAngleTo(car.X + dir.X, car.Y + dir.Y);

      return Math.Abs(angle) > ((Math.PI / 2) + (Math.PI / 18) );///100 degrees
    }

    public override void execute(Move move) {
      move.EnginePower = -1.0;
    }

    public override HashSet<ActionType> blockers { get { return new HashSet<ActionType>() { ActionType.InitialFreeze, ActionType.StuckOut }; } }
  }
}
