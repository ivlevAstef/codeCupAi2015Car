using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_ForwardAction : A_M_BaseMoveAction {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      return true;
    }

    public override void execute(Move move) {
      Vector dir = new Vector(path[0].DirOut.X, path[0].DirOut.Y);
      Vector perpendicular = new Vector(path[0].DirIn.X, path[0].DirIn.Y);

      Vector speed = new Vector(car.SpeedX, car.SpeedY).Normalize();
      Vector speedNormal = new Vector(Math.Cos(car.Angle), Math.Sin(car.Angle)).Normalize();
      double negativeSpeed = speed.Dot(perpendicular) - speedNormal.Dot(perpendicular);

      if (negativeSpeed > 0 && !path[0].DirOut.Equals(path[0].DirIn)) {
        double dx = dir.X / negativeSpeed - perpendicular.X * negativeSpeed;
        double dy = dir.Y / negativeSpeed - perpendicular.Y * negativeSpeed;
        double angle = car.GetAngleTo(car.X + dx, car.Y + dy);
        move.WheelTurn = car.WheelTurnForAngle(angle, game);
      } else {
        double angle = magniteToSide();
        move.WheelTurn = car.WheelTurnForAngle(angle, game);
      }

      move.EnginePower = 1.0;
    }

    public override HashSet<ActionType> GetParallelsActions() {
      HashSet<ActionType> result = new HashSet<ActionType>() {
        ActionType.PreTurn,
        ActionType.Shooting
      };

      bool smallAngleDeviation = Math.Abs(magniteToSide()) < Math.PI / 12;
      if (isStraight() && smallAngleDeviation) {
        result.Add(ActionType.UseNitro);
      }

      return result;
    }

    private bool isStraight() {
      int straightCount = 0;
      for (int i = 0; i < Math.Min(5, path.Count); i++) {
        if (path[i].DirIn.Equals(path[i].DirOut)) {
          straightCount++;
        } else {
          break;
        }
      }

      return straightCount >= 5;
    }
    private double magniteToSide() {
      PointInt pos = path[0].Pos;
      PointInt dir = path[0].DirOut;
      PointInt normal = new PointInt(0);

      for (int i = 1; i < Math.Min(8, path.Count); i++) {
        if (null == path[i].DirOut) {
          break;
        }

        if (!path[i].DirIn.Equals(path[i].DirOut)) {
          normal = path[i].DirOut;
          break;
        }
        pos = path[i].Pos;
        dir = path[i].DirIn;
      }

      return magniteToSide(pos, dir, normal.Negative());
    }

    private double magniteToSide(PointInt pos, PointInt dir, PointInt normal) {
      double sideDistance = (game.TrackTileSize * 0.5) - game.TrackTileMargin - game.CarHeight * 0.55;

      double centerX = (pos.X +0.5) * game.TrackTileSize;
      double centerY = (pos.Y +0.5) * game.TrackTileSize;

      double sideX = centerX + normal.X * sideDistance;
      double sideY = centerY + normal.Y * sideDistance;

      return car.GetAngleTo(sideX, sideY);
    }
  }
}
