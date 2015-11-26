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

      Vector dir = null;

      double angle = car.GetAngleTo(car.X + dirEnd.X, car.Y + dirEnd.Y);
       if (isEndAtAngle(angle)) {
        move.WheelTurn = car.WheelTurnForAngle(angle, game);
        dir = new Vector(dirEnd.X, dirEnd.Y);
      } else {
        Vector endPoint = GetWaySideEnd(path[0].Pos, dirMove, dirEnd);

        angle = car.GetAngleTo(endPoint.X, endPoint.Y);
        move.WheelTurn = car.WheelTurnForAngle(angle, game);
        dir = new Vector(dirMove.X + dirEnd.X, dirMove.Y + dirEnd.Y);
      }

      if (Constant.isExceedMaxTurnSpeed(car, dir.Perpendicular(), 0.6)) {
        move.EnginePower = Constant.MaxTurnSpeed(car, 0.6) / car.Speed();
        move.IsBrake = true;
      } else {
        move.EnginePower = 1.0;
      }
    }

    public override HashSet<ActionType> GetParallelsActions() {
      return new HashSet<ActionType>() {
        ActionType.Shooting,
        ActionType.OilSpill,
      };
    }
  }
}
