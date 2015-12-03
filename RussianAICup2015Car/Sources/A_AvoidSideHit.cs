using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_AvoidSideHit : A_BaseAction {
    private const int MaxCheckTicks = 30;

    public override bool valid() {

      PhysicCar self = new PhysicCar(car, game);
      List<PhysicCar> enemies = new List<PhysicCar>();
      foreach (Car iter in world.Cars) {
        if (iter.Id != car.Id && iter.Speed() > 8) {
          enemies.Add(new PhysicCar(iter, game));
        }
      }

      return checkSideHit(self, enemies);
    }

    public override void execute(Move move) {
      move.IsBrake = true;
    }

    private bool checkSideHit(PhysicCar self, List<PhysicCar> enemies) {
      double checkRadius = (game.CarHeight + game.CarWidth) * Math.Sqrt(2) * 0.5;
      const double maxAngle = Math.PI / 4;

      for (int i = 0; i < MaxCheckTicks; i++) {
        self.Iteration(1);
        foreach (PhysicCar enemy in enemies) {
          enemy.Iteration(1);

          Vector distance = enemy.Pos - self.Pos;
          double angle = Math.Abs(Math.Acos(distance.Normalize().Dot(self.Dir)));
          double angleSpeed = Math.Abs(Math.Acos(self.Dir.Dot(enemy.Dir)));
          if (distance.Length < checkRadius && angle < maxAngle && angleSpeed < maxAngle) {
            return true;
          }

        }
      }

      return false;
    }
  }
}
