using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources.Actions.Moving {
  class AvoidSideHitMoving : MovingBase {
    private const int MinCheckTicks = 10;
    private const int MaxCheckTicks = 80;

    public override bool valid() {

      Physic.PCar self = new Physic.PCar(car, game);
      List<Physic.PCar> enemies = new List<Physic.PCar>();
      foreach (Car iter in world.Cars) {
        if (iter.Id != car.Id) {
          enemies.Add(new Physic.PCar(iter, game));
        }
      }

      return checkSideHit(self, enemies);
    }

    public override void execute(Move move) {
      move.IsBrake = car.Speed() > Constant.MinBrakeSpeed;
    }

    private bool checkSideHit(Physic.PCar self, List<Physic.PCar> enemies) {
      double checkRadius = (game.CarHeight + game.CarWidth) * Math.Sqrt(2) * 0.5;
      double maxAngle = Math.PI / 6;

      self.setEnginePower(1);
      for (int i = 0; i < MaxCheckTicks; i++) {
        self.Iteration(1);
        foreach (Physic.PCar enemy in enemies) {
          enemy.Iteration(1);

          Vector distance = enemy.Pos - self.Pos;
          double angle = Math.Abs(distance.Normalize().Cross(self.Dir));
          double angleSpeed = Math.Abs(self.Dir.Dot(enemy.Dir));
          if (i > MinCheckTicks && distance.Length < checkRadius && angle.LessDotWithAngle(maxAngle) && angleSpeed.LessDotWithAngle(maxAngle)) {
            return true;
          }

        }
      }

      return false;
    }
  }
}
