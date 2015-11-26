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
        return true;
      }

      return dirIn.Equals(dirOut.Perpendicular()) || dirIn.Equals(dirOut.Perpendicular().Negative()); 
    }

    public override void execute(Move move) {
      PointInt dirMove = path[2].DirIn;
      PointInt dirOut = path[2].DirOut;
      Vector dir = new Vector(dirMove.X + dirOut.X, dirMove.Y + dirOut.Y);

      if (Constant.isExceedMaxTurnSpeed(car, dir.Perpendicular(), 0.3)) {
        move.EnginePower = Constant.MaxTurnSpeed(car, 0.3) / car.Speed();
        move.IsBrake = true;
      } else {
        move.EnginePower = 1.0;
      }

      double magnitedAngle = car.GetAngleTo(car.X + dirMove.X, car.Y + dirMove.Y);
      if (isEndAtAngle(magnitedAngle)) {
        move.WheelTurn = car.WheelTurnForAngle(magnitedAngle, game);
      }
    }
  }
}
