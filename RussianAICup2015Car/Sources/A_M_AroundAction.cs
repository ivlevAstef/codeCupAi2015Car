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

      MoveEndType moveEndType = isEndAtAngle(car.GetAngleTo(car.X + dirEnd.X, car.Y + dirEnd.Y));
      if (MoveEndType.Success == moveEndType) {
        double angle = car.GetAngleTo(car.X + dirEnd.X - dirMove.X, car.Y + dirEnd.Y - dirMove.Y);
        move.WheelTurn = car.WheelTurnForAngle(angle, game);
      } else {
        Vector endPoint = GetWaySideEnd(path[0].Pos, dirMove, dirEnd);
        Vector endPointReverse = GetWaySideEnd(path[0].Pos, dirMove, dirEnd.Negative());

        if(isEndAt(10 + car.TicksForAngle(car.GetAngleTo(endPoint.X, endPoint.Y), game))) {
          double angle = car.GetAngleTo(endPoint.X, endPoint.Y);
          move.WheelTurn = car.WheelTurnForAngle(angle, game);
        } else {
          double angle = car.GetAngleTo(endPointReverse.X, endPointReverse.Y);
          move.WheelTurn = car.WheelTurnForAngle(angle, game);
        }
      }

      Vector dir = new Vector(dirEnd.X - dirMove.X, dirEnd.Y - dirMove.Y);
      double exceed = 0;
      double enginePowerConst = 0.1;
      if (MoveEndType.Success == moveEndType) {
        exceed = Constant.ExceedMaxTurnSpeed(car, dir.Perpendicular(), 0.9);
        enginePowerConst = 2.0;
      } else if (MoveEndType.SideCrash == moveEndType) {
        exceed = Constant.ExceedMaxTurnSpeed(car, dir.Perpendicular(), 0.4);
        enginePowerConst = 0.8;
      } else {
        exceed = Constant.ExceedMaxTurnSpeed(car, dir.Perpendicular(), 0.1);
        enginePowerConst = 0.3;
      }

      if (exceed > 0) {
        move.EnginePower = Constant.MaxTurnSpeed(car, enginePowerConst) / car.Speed();
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
