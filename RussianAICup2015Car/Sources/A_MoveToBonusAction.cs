using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_MoveToBonusAction : A_BaseAction {
    private Bonus findedBonus = null;

    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      findedBonus = findBonus();

      return null != findedBonus;
    }

    public override void execute(Move move) {
      PointInt dirMove = path[0].DirOut;

      double roughAngle = car.GetAngleTo(findedBonus);
      double sign = Math.Sign(roughAngle);

      double angle = roughAngle;
      if (Math.Abs(roughAngle) > Math.PI / 18) {
        double x = findedBonus.X + sign * dirMove.Y * (findedBonus.Height * 0.5 + car.Height * 0.45);
        double y = findedBonus.Y - sign * dirMove.X * (findedBonus.Height * 0.5 + car.Height * 0.45);

        angle = car.GetAngleTo(x, y);
      }

      move.WheelTurn = car.WheelTurnForAngle(angle, game);

      move.EnginePower = 1.0;
    }

    public override HashSet<ActionType> GetParallelsActions() {
      return new HashSet<ActionType>() {
        ActionType.Shooting
      };
    }

    private Bonus findBonus() {
      PointInt dir = path[0].DirOut;

      Dictionary<BonusType, int> priority = new Dictionary<BonusType, int> {
        { BonusType.AmmoCrate , Math.Min(10, 70 - 10 * car.ProjectileCount) },
        { BonusType.NitroBoost , Math.Min(10, 80 - 10 * car.NitroChargeCount) },
        { BonusType.OilCanister , Math.Min(10, 50 - 10 * car.OilCanisterCount) },
        { BonusType.PureScore , 100 },
        { BonusType.RepairKit , (int)(150 * (1.0 - car.Durability)) }
      };

      if (car.Speed() < 1) {
        return null;
      }

      double speed = car.Speed();
      double maxAngle = Math.PI / (9 * Math.Min(0.75, speed / (game.TrackTileSize / 80)));

      Bonus priorityBonus = null;
      foreach (Bonus bonus in world.Bonuses) {
        double distance = car.GetDistanceTo(bonus);
        if (distance > game.TrackTileSize * 4.0) {
          continue;
        }

        double angle = car.GetAbsoluteAngleTo(bonus.X, bonus.Y, dir.X, dir.Y);
        if (Math.Abs(angle) > maxAngle) {
          continue;
        }

        PointInt selfTile = tilePos(car.X, car.Y);
        PointInt bonusTile = tilePos(bonus.X, bonus.Y);
        if (!selfTile.Equals(bonusTile) && !(selfTile + dir).Equals(bonusTile)) {
          continue;
        }

        if (null == priorityBonus || priority[priorityBonus.Type] < priority[bonus.Type]) {
          priorityBonus = bonus;
        }
      }

      return priorityBonus;
    }

    private PointInt tilePos(double x, double y) {
      return new PointInt((int)(x / game.TrackTileSize), (int)(y / game.TrackTileSize));
    }
  }
}
