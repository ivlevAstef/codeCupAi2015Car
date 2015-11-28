using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_MoveToBonusAction : A_BaseAction {
    private Bonus findedBonus = null;

    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      findedBonus = findBonus();

      return null != findedBonus && angleToBonus(findedBonus) < Math.PI / 9;
    }

    public override void execute(Move move) {
      Logger.instance.Assert(null != findedBonus, "Didn't find bonus.");

      move.WheelTurn = car.WheelTurnForAngle(angleToBonus(findedBonus), game);

      move.EnginePower = 1.0;
    }

    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.PreTurn,
        ActionType.Shooting
      };
    }

    private double angleToBonus(Bonus bonus) {
      Vector dirMove = new Vector(path[0].DirOut.X, path[0].DirOut.Y);
      Vector center = new Vector((path[0].Pos.X + 0.5), (path[0].Pos.Y + 0.5)) * game.TrackTileSize;

      double dirAngle = Math.Atan2(dirMove.Y, dirMove.X);

      double centerX = car.X * Math.Abs(dirMove.X) + center.X * Math.Abs(dirMove.Y);
      double centerY = car.Y * Math.Abs(dirMove.Y) + center.Y * Math.Abs(dirMove.X);

      double centerAngle = new Vector(centerX, centerY).GetAngleTo(bonus.X, bonus.Y, dirAngle);
      double sign = Math.Sign(centerAngle);

      double x = bonus.X + sign * dirMove.Y * (bonus.Height * 0.25 + car.Height * 0.5);
      double y = bonus.Y - sign * dirMove.X * (bonus.Height * 0.25 + car.Height * 0.5);

      return car.GetAngleTo(x, y);
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
