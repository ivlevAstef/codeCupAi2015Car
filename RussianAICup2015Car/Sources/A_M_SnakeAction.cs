using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_SnakeAction : A_M_BaseMoveAction {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      PointInt posIn = path[1].Pos;
      PointInt posOut = path[2].Pos;

      PointInt dirIn = path[1].DirIn;
      PointInt dirOut = path[2].DirOut;

      if (null == dirOut || dirOut.Equals(new PointInt(0))) {
        return false;
      }

      PointInt dir = new PointInt(posOut.X - posIn.X, posOut.Y - posIn.Y);

      return dirIn.Equals(dirOut) && (dir.Equals(dirIn.Perpendicular()) || dir.Equals(dirIn.Perpendicular().Negative()));
    }

    public override void execute(Move move) {
      PointInt dirMove = path[0].DirOut;
      PointInt dirEnd = path[1].DirOut;

      double magnitedAngle = magniteToCenter(dirMove, dirEnd);

      Vector dir = new Vector(dirMove.X + dirEnd.X, dirMove.Y + dirEnd.Y);
      if (Constant.isExceedMaxTurnSpeed(car, dir.Perpendicular(), 0.75)) {
        move.EnginePower = Constant.MaxTurnSpeed(car, 0.75) / car.Speed();
        move.IsBrake = true;
      } else {
        move.EnginePower = 1.0;
      }

      move.WheelTurn = car.WheelTurnForAngle(magnitedAngle, game);
    }

    public override HashSet<ActionType> GetParallelsActions() {
      HashSet<ActionType> result = new HashSet<ActionType>() {
        ActionType.OilSpill,
        ActionType.Shooting
      };

      bool smallAngleDeviation = Math.Abs(magniteToCenter(path[0].DirOut, path[1].DirOut)) < Math.PI / 32;
      if (Math.Abs(car.AngularSpeed) < 0.001 && smallAngleDeviation) {
        result.Add(ActionType.UseNitro);
      }

      return result;
    }

    private double magniteToCenter(PointInt dir1, PointInt dir2) {
      double powerTilt = game.TrackTileSize * 0.45;
      Vector dir = new Vector((dir1.X + dir2.X) / Math.Sqrt(2), (dir1.Y + dir2.Y) / Math.Sqrt(2));

      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5 + 0.5 * dir1.X) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5 + 0.5 * dir1.Y) * game.TrackTileSize;

      return car.GetAngleTo(new Vector(centerX, centerY), dir, powerTilt);
    }
  }
}
