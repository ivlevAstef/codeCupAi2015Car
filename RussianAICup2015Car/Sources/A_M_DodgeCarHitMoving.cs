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

      Vector distance = enemy.Pos - self.Pos;
      double angle = distance.Angle.AngleDeviation(self.Angle);
      double sign = Math.Abs(angle) < Math.Sin(Math.PI/32) ? 1 : Math.Sign(angle);

      Vector endPos = self.Pos + distance.Normalize().PerpendicularLeft() * sign * car.Height;

      TileDir dirMove = path[0].DirOut;
      Physic.MovingCalculator calculator = new Physic.MovingCalculator();
      calculator.setupEnvironment(car, game, world);
      calculator.setupMapInfo(dirMove, path[0].Pos, null);

      calculator.setupAngleReach(new Vector(dirMove.X, dirMove.Y));
      calculator.setupDefaultAction(endPos);

      Move needMove = calculator.calculateMove();
      move.EnginePower = needMove.EnginePower;
      move.WheelTurn = needMove.WheelTurn;
    }

    private Tuple<PCar, PCar> hitInformation() {
      PCar self = new PCar(car, game);
      List<PCar> enemies = new List<PCar>();
      foreach (Car iter in world.Cars) {
        if (iter.Id != car.Id) {
          enemies.Add(new PCar(iter, game));
        }
      }

      return hitInformation(self, enemies);
    }

    private Tuple<PCar,PCar> hitInformation(PCar self, List<PCar> enemies) {
      double maxAngle = Math.Sin(Math.Atan2(car.Height, car.Width));

      self.setEnginePower(1);

      for (int i = 0; i < MaxCheckTicks; i++) {
        self.Iteration(1);
        foreach (PCar enemy in enemies) {
          enemy.Iteration(1);

          Vector distance = enemy.Pos - self.Pos;
          double angle = Math.Abs(distance.Normalize().Cross(self.Dir));
          if (distance.Length < game.CarWidth && angle < maxAngle) {
            return new Tuple<PCar,PCar>(self, enemy);
          }

        }
      }

      return null;
    }
  }
}
