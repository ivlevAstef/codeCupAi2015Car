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
      Logger.instance.Assert(null != findedBonus, "Didn't find bonus.");

      PointInt dirMove = path[0].DirOut;

      Vector endPos = new Vector(findedBonus.X, findedBonus.Y);

      PhysicMoveCalculator calculator = new PhysicMoveCalculator();
      calculator.setupEnvironment(car, game, world);

      Vector dir = new Vector(dirMove.X, dirMove.Y);

      Move needMove = calculator.calculateMove(endPos, dir, dir, 0.05);
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }

    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.PreTurn,
        ActionType.Shooting
      };
    }

    private Bonus findBonus() {
      Vector dir = new Vector(path[0].DirOut.X, path[0].DirOut.Y);

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
      Vector carPos = new Vector(car.X, car.Y);

      Bonus priorityBonus = null;
      foreach (Bonus bonus in world.Bonuses) {
        double distance = car.GetDistanceTo(bonus);
        if (distance > game.TrackTileSize * 3.0) {
          continue;
        }

        Vector bonusPos = new Vector(bonus.X, bonus.Y);

        if ((bonusPos - carPos).Dot(dir) < 0) {//back
          continue;
        }

        if (!isNextTile(tilePos(bonus.X, bonus.Y))) {
          continue;
        }

        if (null == priorityBonus || priority[priorityBonus.Type] < priority[bonus.Type]) {
          priorityBonus = bonus;
        }
      }

      return priorityBonus;
    }

    private bool isNextTile(PointInt checkTile) {
      for (int i = 0; i < Math.Min(3, path.Count); i++) {
        if (path[i].Pos.Equals(checkTile)) {
          return true;
        }
      }
      return false;
    }

    private PointInt tilePos(double x, double y) {
      return new PointInt((int)(x / game.TrackTileSize), (int)(y / game.TrackTileSize));
    }
  }
}
