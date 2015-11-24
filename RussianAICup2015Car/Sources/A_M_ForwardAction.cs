using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_M_ForwardAction : A_BaseAction {
    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      return true;
    }

    public override void execute(Move move) {
      move.EnginePower = 1.0;

      double magnitedAngle = magniteToCenter(path[0].DirOut);
      double magnitedForce = car.WheelTurnForAngle(magnitedAngle, game);

      if (Math.Abs(magnitedAngle) > Math.PI / (3 * car.Speed() / 25)) {
        move.IsBrake = true;
      }

      move.WheelTurn = magnitedForce;
    }

    public override HashSet<ActionType> GetParallelsActions() {
      HashSet<ActionType> result = new HashSet<ActionType>() {
        ActionType.Shooting
      };

      bool smallAngleDeviation = Math.Abs(magniteToCenter(path[0].DirOut)) < Math.PI / 12;
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

    private double magniteToCenter(PointInt dir) {
      double powerTilt = game.TrackTileSize;

      double centerX = (Math.Floor(car.X / game.TrackTileSize) + 0.5) * game.TrackTileSize;
      double centerY = (Math.Floor(car.Y / game.TrackTileSize) + 0.5) * game.TrackTileSize;

      return car.GetAngleTo(new PointDouble(centerX, centerY), dir, powerTilt);
    }
  }
}
