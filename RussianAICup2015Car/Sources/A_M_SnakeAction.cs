using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_SnakeAction : A_M_BaseMoveAction {
    private int goodTicks = 0;

    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");


      return validSnakeWithOffset(0);
    }

    public override void execute(Move move) {
      PointInt dirMove = path[0].DirOut;
      PointInt dirEnd = path[1].DirOut;

      double magnitedAngle = magniteToCenter(dirMove, dirEnd);

      Vector dir = new Vector(dirMove.X + dirEnd.X, dirMove.Y + dirEnd.Y);
      double exceed = Constant.ExceedMaxTurnSpeed(car, dir.Perpendicular(), 0.8);

      if (exceed > 0) {
        move.EnginePower = Constant.MaxTurnSpeed(car, 0.85) / car.Speed();
        move.IsBrake = true;
      } else {
        move.EnginePower = 1.0;
      }

      move.WheelTurn = car.WheelTurnForAngle(magnitedAngle, game);
    }

    public override List<ActionType> GetParallelsActions() {
      List<ActionType> result = new List<ActionType>() {
        ActionType.OilSpill,
        ActionType.Shooting,
        ActionType.SnakePreEnd
      };

      PointInt dirMove = path[0].DirOut;
      PointInt dirEnd = path[1].DirOut;

      Vector dir = new Vector(dirMove.X + dirEnd.X, dirMove.Y + dirEnd.Y);
      if (Constant.ExceedMaxTurnSpeed(car, dir.Perpendicular(), 1.2) < -3 &&
          validSnakeWithOffset(1) && validSnakeWithOffset(2)) {
        goodTicks++;
        if (goodTicks > 5) {
          result.Add(ActionType.UseNitro);
        }
      } else {
        goodTicks = 0;
      }

      return result;
    }

    private bool validSnakeWithOffset(int offset) {
      if (3 + offset >= path.Count) {
        return true;
      }

      PointInt posIn = path[1 + offset].Pos;
      PointInt posOut = path[2 + offset].Pos;

      PointInt dirIn = path[1 + offset].DirIn;
      PointInt dirOut = path[2 + offset].DirOut;

      if (null == dirOut || dirOut.Equals(new PointInt(0))) {
        return false;
      }

      PointInt dir = new PointInt(posOut.X - posIn.X, posOut.Y - posIn.Y);

      return dirIn.Equals(dirOut) && (dir.Equals(dirIn.PerpendicularLeft()) || dir.Equals(dirIn.PerpendicularRight()));
    }

    private double magniteToCenter(PointInt dir1, PointInt dir2) {
      double powerTilt = game.TrackTileSize * 0.45;
      Vector dir = new Vector(dir1.X + dir2.X, dir1.Y + dir2.Y).Normalize();

      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5 + 0.5 * dir1.X) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5 + 0.5 * dir1.Y) * game.TrackTileSize;

      return car.GetAngleTo(new Vector(centerX, centerY), dir, powerTilt);
    }
  }
}
