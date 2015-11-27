using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_AroundAction : A_M_BaseMoveAction {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      PointInt posIn = path[1].Pos;
      PointInt posOut = path[2].Pos;

      PointInt dirIn = path[1].DirIn;
      PointInt dirOut = path[2].DirOut;

      if (null == dirOut) {
        return false;
      }

      return dirIn.Equals(dirOut.Negative()) && !posIn.Equals(posOut);
    }

    public override void execute(Move move) {
      PointInt dirMove = path[0].DirOut;
      PointInt dirEnd = path[1].DirOut;

      Vector dir = new Vector(dirMove.X + dirEnd.X, dirMove.Y + dirEnd.Y);
      double angleEnd = car.GetAngleTo(car.X + dirEnd.X, car.Y + dirEnd.Y);
      if (isEndAtAngle(angleEnd)) {
        move.WheelTurn = car.WheelTurnForAngle(angleEnd, game);
        dir = new Vector(dirEnd.X, dirEnd.Y);
      } else {
        if(isEndAt(20)) {
          Vector endPoint = GetWaySideEnd(path[0].Pos, dirMove, dirEnd);
          double angle = car.GetAngleTo(endPoint.X, endPoint.Y);
          move.WheelTurn = car.WheelTurnForAngle(angle, game);
        } else {
          Vector endPointReverse = GetWaySideEnd(path[0].Pos, dirMove, dirEnd.Negative());
          double angle = car.GetAngleTo(endPointReverse.X, endPointReverse.Y);
          move.WheelTurn = car.WheelTurnForAngle(angle, game);
        }
        dir = new Vector(dirMove.X + dirEnd.X, dirMove.Y + dirEnd.Y);
      }

      if (Constant.isExceedMaxTurnSpeed(car, dir.Perpendicular(), 1.2)) {
        move.EnginePower = Constant.MaxTurnSpeed(car, 1.2) / car.Speed();
        move.IsBrake = true;
      } else {
        move.EnginePower = 1.0;
      }
    }

    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.Shooting,
        ActionType.OilSpill
      };
    }
  }
}
