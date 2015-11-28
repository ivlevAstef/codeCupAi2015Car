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

      double angle = car.GetAngleTo(car.X + dirEnd.X, car.Y + dirEnd.Y);
      MoveEndType moveEndType = isEndAtAngle(angle);

      if (MoveEndType.Success == moveEndType) {
        move.WheelTurn = car.WheelTurnForAngle(angle, game);
      } else if (MoveEndType.SideCrash == moveEndType) {
        Vector endPoint = GetWayEnd(path[0].Pos, dirMove);
        angle = car.GetAngleTo(endPoint.X, endPoint.Y);
        move.WheelTurn = car.WheelTurnForAngle(angle*0.5, game);
      } else {
        Vector endPoint = GetWaySideEnd(path[0].Pos, dirMove, dirEnd);

        angle = car.GetAngleTo(endPoint.X, endPoint.Y);
        move.WheelTurn = car.WheelTurnForAngle(angle, game);
      }

      double distanceToEnd = (GetWayEnd(path[0].Pos, dirMove) - new Vector(car.X, car.Y)).Dot(new Vector(dirMove.X, dirMove.Y));
      double speedConstant = 5 * distanceToEnd / game.TrackTileSize;

      double exceed = 0;
      Vector dir = null;
      if (MoveEndType.Success == moveEndType) {
        dir = new Vector(dirEnd.X, dirEnd.Y);
        exceed = Constant.ExceedMaxTurnSpeed(car, dir.Perpendicular(), 0.5);
      } else if(MoveEndType.SideCrash == moveEndType) {
        dir = new Vector(dirEnd.X, dirEnd.Y);
        exceed = Constant.ExceedMaxTurnSpeed(car, dir.Perpendicular(), 0.9);
      } else {
        dir = new Vector(dirMove.X + dirEnd.X, dirMove.Y + dirEnd.Y);
        exceed = Constant.ExceedMaxTurnSpeed(car, dir.Perpendicular(), 0.55);
      }

      if (exceed > speedConstant) {
        move.EnginePower = Constant.MaxTurnSpeed(car, 0.9) / car.Speed();
        move.IsBrake = exceed > speedConstant + 1;
      } else {
        move.EnginePower = 1.0;
      }
    }

    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.Shooting,
        ActionType.OilSpill,
      };
    }
  }
}
