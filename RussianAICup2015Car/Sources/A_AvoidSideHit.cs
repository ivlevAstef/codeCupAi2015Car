using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_AvoidSideHit : A_BaseAction {
    private const int MaxCheckTicks = 40;

    public override bool valid() {

      PhysicCar self = new PhysicCar(car, game);
      List<PhysicCar> enemies = new List<PhysicCar>();
      foreach (Car iter in world.Cars) {
        if (iter.Id != car.Id) {
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
      double maxAngle = Math.Sin(Math.PI / 6);

      self.setEnginePower(1);
      foreach (PhysicCar enemy in enemies) {
        enemy.setEnginePower(1);
      }

      for (int i = 0; i < MaxCheckTicks; i++) {
        self.Iteration(1);
        foreach (PhysicCar enemy in enemies) {
          enemy.Iteration(1);

          Vector distance = enemy.Pos - self.Pos;
          double angle = Math.Abs(distance.Normalize().Cross(self.Dir));
          double angleSpeed = Math.Abs(self.Dir.Dot(enemy.Dir));
          if (distance.Length < checkRadius && angle < maxAngle && angleSpeed < maxAngle) {
            return true;
          }

        }
      }

      return false;
    }
  }
}
