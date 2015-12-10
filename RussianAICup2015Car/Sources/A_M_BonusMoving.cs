using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class BonusMoving : MovingBase {
    private Bonus findedBonus = null;

    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      findedBonus = findBonus();

      return null != findedBonus;
    }

    public override void execute(Move move) {
      Logger.instance.Assert(null != findedBonus, "Didn't find bonus.");

      TileDir dirMove = path[0].DirOut;

      Vector endPos = bonusEndPos(findedBonus, dirMove);

      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);

      Vector dir = new Vector(dirMove.X, dirMove.Y);

      Move needMove = calculator.calculateMove(endPos, dirMove, dir);
      if (!needMove.IsBrake) {
        move.EnginePower = needMove.EnginePower;
        move.WheelTurn = needMove.WheelTurn;
      }
    }

    public override List<ActionType> GetParallelsActions() {
      return new List<ActionType>() {
        ActionType.PreTurn,
        ActionType.Shooting
      };
    }

    private Vector bonusEndPos(Bonus bonus, TileDir dirMove) {
      Vector center = new TilePos(bonus.X, bonus.Y).ToVector(0.5, 0.5);
      Vector endPos = new Vector(findedBonus.X, findedBonus.Y);

      Vector perpendicular = new Vector(dirMove.X, dirMove.Y).Perpendicular();
      double sign = Math.Sign((endPos - center).Dot(perpendicular));

      endPos = endPos - perpendicular * (sign * game.CarHeight * 0.5);

      return endPos;
    }

    private Bonus findBonus() {
      Vector dir = new Vector(path[0].DirOut.X, path[0].DirOut.Y);

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

        if (!isNextTile(new TilePos(bonus.X, bonus.Y))) {
          continue;
        }

        if (null == priorityBonus || bonusPriorityFor(priorityBonus) < bonusPriorityFor(bonus)) {
          priorityBonus = bonus;
        }
      }

      return priorityBonus;
    }

    private double bonusPriorityFor(Bonus bonus) {
      Dictionary<BonusType, int> priority = new Dictionary<BonusType, int> {
        { BonusType.AmmoCrate , Math.Min(10, 70 - 10 * car.ProjectileCount) },
        { BonusType.NitroBoost , Math.Min(10, 80 - 10 * car.NitroChargeCount) },
        { BonusType.OilCanister , Math.Min(10, 50 - 10 * car.OilCanisterCount) },
        { BonusType.PureScore , 100 },
        { BonusType.RepairKit , (int)(150 * (1.0 - car.Durability)) }
      };

      double distance = car.GetDistanceTo(bonus);

      return priority[bonus.Type] - (distance * 0.05);
    }

    private bool isNextTile(TilePos checkTile) {
      for (int i = 0; i < Math.Min(3, path.Count); i++) {
        if (path[i].Pos == checkTile) {
          return true;
        }
      }
      return false;
    }
  }
}
