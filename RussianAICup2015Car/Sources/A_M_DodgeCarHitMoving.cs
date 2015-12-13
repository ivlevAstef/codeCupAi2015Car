using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using RussianAICup2015Car.Sources.Physic;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class DodgeCarHitMoving : MovingBase {
    private const int MaxCheckTicks = 50;

    private Tuple<PCar, PCar> hitInfo = null;

    public override bool valid() {
      hitInfo = hitInformation();

      return null != hitInfo;
    }

    public override void execute(Move move) {
      PCar self = hitInfo.Item1;
      PCar enemy = hitInfo.Item2;

      Vector selfPos = new Vector(self.Car.X, self.Car.Y);
      Vector enemyPos = new Vector(enemy.Car.X, enemy.Car.Y);

      double angle = self.Car.Angle.AngleDeviation((enemy.Pos - selfPos).Angle);
      double sign = Math.Sign(angle);

      Vector center = selfPos + (enemyPos - selfPos) * 0.5;
      Vector dir = new Vector(path[0].DirOut.X, path[0].DirOut.Y);
      Vector endPos = center + dir.PerpendicularRight() * sign * car.Height;

      TileDir dirMove = path[0].DirOut;
      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);
      calculator.setupMapInfo(dirMove, path[0].Pos, null);

      calculator.setupAngleReach(new Vector(dirMove.X, dirMove.Y));
      calculator.setupDefaultAction(endPos);

      Dictionary<TilePos, TileDir[]> selfMap = new Dictionary<TilePos, TileDir[]>();
      for (int i = 0; i <= 2; i++) {
        if (path[i].DirIn == path[i].DirOut) {
          selfMap.Add(path[i].Pos, new TileDir[2] { dirMove.PerpendicularLeft(), dirMove.PerpendicularRight() });
        } else if (0 == i) {
          selfMap.Add(path[i].Pos, new TileDir[1] { path[i].DirIn });
        } else {
          break;
        }
      }
      calculator.setupSelfMapCrash(selfMap);

      Move needMove = calculator.calculateMove();
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }

    private Tuple<PCar, PCar> hitInformation() {
      PCar self = new PCar(car, game);
      List<PCar> enemies = new List<PCar>();
      foreach (Car iter in world.Cars) {
        Vector distance = new Vector(iter.X, iter.Y) - self.Pos;
        if (iter.Id != car.Id && distance.Dot(self.Dir) > 0) {
          enemies.Add(new PCar(iter, game));
        }
      }

      return hitInformation(self, enemies);
    }

    private Tuple<PCar,PCar> hitInformation(PCar self, List<PCar> enemies) {
      //double maxAngle = Math.Atan2(car.Height, car.Width);
      double maxAngle = Math.PI / 2.57;//70degrees

      self.setEnginePower(1);
      MoveToAngleFunction mover = new MoveToAngleFunction(new Vector(path[0].DirOut.X, path[0].DirOut.Y).Angle);

      for (int i = 0; i < MaxCheckTicks; i++) {
        foreach (PCar enemy in enemies) {
          Vector distance = enemy.Pos - self.Pos;
          double angle = Math.Abs(Math.Acos(distance.Normalize().Dot(self.Dir)));
          if (distance.Length < game.CarWidth && angle < maxAngle) {
            return new Tuple<PCar,PCar>(self, enemy);
          }
          enemy.Iteration(1);
        }
        mover.Iteration(self, 1);
      }

      return null;
    }
  }
}
