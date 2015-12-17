using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using RussianAICup2015Car.Sources.Physic;

namespace RussianAICup2015Car.Sources.Actions {
  class BonusMoving : AdditionalPoints {
    public override List<Tuple<Vector, double>> GetPoints() {
      Bonus findedBonus = findBonus();
      if (null == findedBonus) {
        return null;
      }

      TileDir dirMove = path[0].DirOut;

      return new List<Tuple<Vector, double>> { 
        new Tuple<Vector,double>(bonusEndPos(findedBonus, dirMove), car.Height * 0.5)
      };
    }

    private Vector bonusEndPos(Bonus bonus, TileDir dirMove) {
      Vector center = new TilePos(bonus.X, bonus.Y).ToVector(0.5, 0.5);
      Vector endPos = new Vector(bonus.X, bonus.Y);

      Vector perpendicular = new Vector(dirMove.X, dirMove.Y).Perpendicular();
      double sign = Math.Sign((endPos - center).Dot(perpendicular));

      endPos = endPos - perpendicular * (sign * game.BonusSize * 0.5);

      return endPos;
    }

    private Bonus findBonus() {
      Vector dir = new Vector(path[0].DirOut.X, path[0].DirOut.Y);

      PCar pcar = new PCar(car, game);

      if (pcar.Speed.Length < 1) {
        return null;
      }

      PCar zeroWheelTurnCar = pcar.GetZeroWheelTurnCar();

      Bonus priorityBonus = null;
      foreach (Bonus bonus in world.Bonuses) {
        double distance = car.GetDistanceTo(bonus);
        if (distance > game.TrackTileSize * 3.0) {
          continue;
        }

        Vector bonusPos = new Vector(bonus.X, bonus.Y);

        if ((bonusPos - pcar.Pos).Dot(dir) < 0) {//back
          continue;
        }

        if (!isNextTile(new TilePos(bonus.X, bonus.Y))) {
          continue;
        }

        if (Constant.BonusPriority(bonus, car, false) < 0) {
          continue;
        }

        double newBonusPriority = Constant.BonusPriority(bonus, car, true);

        if (null == priorityBonus || Constant.BonusPriority(priorityBonus, car, true) < newBonusPriority) {
          priorityBonus = bonus;
        }
      }

      return priorityBonus;
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
