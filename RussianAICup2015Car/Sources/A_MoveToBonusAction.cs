using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_MoveToBonusAction : A_BaseAction {
    private Bonus findedBonus = null;

    public override bool valid() {
      findedBonus = findBonus();

      return null != findedBonus;
    }

    public override void execute(Move move) {
      double angle = car.GetAngleTo(findedBonus);

      move.WheelTurn = 0.2 * car.WheelTurnForAngle(angle, game);

      move.EnginePower = 1.0;
    }

    public override HashSet<ActionType> blockers { get { return new HashSet<ActionType>() { 
      ActionType.InitialFreeze,
      ActionType.Turn,
      ActionType.PreTurn,
      ActionType.Snake,
      ActionType.Around,
      ActionType.StuckOut,
    }; } }

    private Bonus findBonus() {
      PointInt dir = path.FirstWayCell.DirOut;

      Dictionary<BonusType, int> priority = new Dictionary<BonusType, int> {
        { BonusType.AmmoCrate , Math.Min(10, 70 - 10 * car.ProjectileCount) },
        { BonusType.NitroBoost , Math.Min(10, 80 - 10 * car.NitroChargeCount) },
        { BonusType.OilCanister , Math.Min(10, 50 - 10 * car.OilCanisterCount) },
        { BonusType.PureScore , 100 },
        { BonusType.RepairKit , (int)(150 * (1.0 - car.Durability)) }
      };

      double speed = car.Speed();
      double maxAngle = Math.PI / (9 * Math.Min(0.75, speed / (game.TrackTileSize / 80)));

      Bonus priorityBonus = null;
      foreach (Bonus bonus in world.Bonuses) {
        double distance = car.GetDistanceTo(bonus);
        if (3 * game.CarWidth > distance || distance > game.TrackTileSize * 2.0) {
          continue;
        }

        double angle = car.GetAngleTo(bonus);
        if (Math.Abs(angle) > maxAngle) {
          continue;
        }

        PointInt selfTile = tilePos(car.X, car.Y);
        PointInt bonusTile = tilePos(bonus.X, bonus.Y);
        if (!selfTile.Equals(bonusTile) && !selfTile.Add(dir).Equals(bonusTile)) {
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
