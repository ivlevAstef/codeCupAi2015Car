using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using RussianAICup2015Car.Sources.Physic;

namespace RussianAICup2015Car.Sources.Actions {
  class DodgeCarHitMoving : AdditionalPoints {
    private const int MaxCheckTicks = 50;

    public override List<Tuple<Vector, double>> GetPoints() {
      Tuple<PCar, PCar> hitInfo = hitInformation();

      if (null == hitInfo) {
        return null;
      }

      PCar self = hitInfo.Item1;
      PCar enemy = hitInfo.Item2;

      Vector selfPos = new Vector(self.Car.X, self.Car.Y);
      Vector enemyPos = new Vector(enemy.Car.X, enemy.Car.Y);

      double angle = self.Car.Angle.AngleDeviation((enemy.Pos - selfPos).Angle);
      double sign = Math.Sign(angle);

      Vector center = selfPos + (enemyPos - selfPos) * 0.5;
      Vector dir = new Vector(path[0].DirOut.X, path[0].DirOut.Y);
      Vector endPos = center + dir.PerpendicularRight() * sign * car.Height;

      return new List<Tuple<Vector, double>> { new Tuple<Vector,double>(endPos, car.Height) };
    }

    private Tuple<PCar, PCar> hitInformation() {
      PCar self = new PCar(car, game);
      List<PCar> enemies = new List<PCar>();
      foreach (Car iter in world.Cars) {
        Vector distance = new Vector(iter.X, iter.Y) - self.Pos;
        if (iter.Id != car.Id && distance.Dot(self.Dir) > 0 && !iter.IsFinishedTrack) {
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
