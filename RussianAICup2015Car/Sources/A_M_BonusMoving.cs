using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using RussianAICup2015Car.Sources.Physic;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class BonusMoving : MovingBase {
    private Bonus findedBonus = null;
    private static double MaxAngle = Math.PI / 6;//30 degrees

    public override bool valid() {
      Logger.instance.Assert(3 <= path.Count, "incorrect way cells count.");

      findedBonus = findBonus();

      return null != findedBonus;
    }

    public override void execute(Move move) {
      Logger.instance.Assert(null != findedBonus, "Didn't find bonus.");

      TileDir dirMove = path[0].DirOut;

      MovingCalculator calculator = new MovingCalculator();
      calculator.setupEnvironment(car, game, world);
      calculator.setupMapInfo(dirMove, path[0].Pos, null);

      calculator.setupAngleReach(new Vector(dirMove.X, dirMove.Y));
      calculator.setupDefaultAction(bonusEndPos(findedBonus, dirMove));

      Move needMove = calculator.calculateMove();
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
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

        double angleToBonus = (bonusPos - pcar.Pos).Angle;
        if (Math.Abs(zeroWheelTurnCar.Angle.AngleDeviation(angleToBonus)) > MaxAngle) {
          continue;
        }

        if (!isNextTile(new TilePos(bonus.X, bonus.Y))) {
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
