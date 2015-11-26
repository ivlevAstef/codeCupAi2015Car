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
      move.EnginePower = 1.0;

      double magnitedAngle = magniteToSide();
      double magnitedForce = car.WheelTurnForAngle(magnitedAngle, game);

      if (Math.Abs(magnitedAngle) > Math.PI / (3 * car.Speed() / 25)) {
        move.IsBrake = true;
      }

      move.WheelTurn = magnitedForce;
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
