using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_PreTurnAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      if (!path[1].DirIn.Equals(path[1].DirOut)) {
        return false;
      }

      PointInt dirIn = path[2].DirIn;
      PointInt dirOut = path[2].DirOut;

      if (null == dirOut) {
        return true;
      }

      return dirIn.Equals(dirOut.Perpendicular()) || dirIn.Equals(dirOut.Perpendicular().Negative()); 
    }

    public override void execute(Move move) {
      PointInt dirMove = path[0].DirOut;

      if (car.Speed() > 32 - car.EnginePower * 5 - car.RemainingNitroTicks * 0.05) {
        move.IsBrake = true;
      }

      double magnitedAngle = car.GetAngleTo(car.X+dirMove.X, car.Y+dirMove.Y);

      move.WheelTurn = car.WheelTurnForAngle(magnitedAngle, game);
    }
  }
}
