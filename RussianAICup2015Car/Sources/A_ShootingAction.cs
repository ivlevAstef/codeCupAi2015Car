using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_ShootingAction : A_BaseAction {
    public override bool valid() {
      if (0 < car.RemainingProjectileCooldownTicks || car.ProjectileCount <= 0) {
        return false;
      }

      if (CarType.Buggy == car.Type) {
        return hasEnemyOnWasherLine();
      }

      return false;
    }

    public override void execute(Move move) {
      move.IsThrowProjectile = true;
    }    

    private bool hasEnemyOnWasherLine() {
      foreach (Car carIter in world.Cars) {
        if (carIter.IsTeammate || carIter.IsFinishedTrack || carIter.Durability <= 1.0e-9) {
          continue;
        }

        if (hasEnemyOnWasherLine(carIter)) {
          return true;
        }
      }

      return false;
    }

    private bool hasEnemyOnWasherLine(Car enemy) {
      PhysicCar physicCar = new PhysicCar(enemy, game);

      Vector washerPos = new Vector(car.X, car.Y);
      Vector washerDir = Vector.sincos(car.Angle);
      Vector washerSpd = washerDir * game.WasherInitialSpeed;

      double radius = Math.Min(car.Width, car.Height) * 0.25 + game.WasherRadius;

      double maxDistance = Math.Max(car.Width, car.Height) / Math.Sin(game.SideWasherAngle);
      int maxTicks = (int)(maxDistance / game.WasherInitialSpeed);

      //for one 30 for two 15 other 0. 
      int ticks = maxTicks - 15 * Math.Max(0, 3 - car.RemainingProjectileCooldownTicks);
      ticks = Math.Max((int)(1.5 * game.TrackTileSize / game.WasherInitialSpeed), ticks);

      for (int i = 0; i < ticks; i++) {
        physicCar.Iteration(1);
        washerPos = washerPos + washerSpd;

        double distanceByDir = (physicCar.Pos - washerPos).Dot(washerDir);
        double fullDistance = physicCar.Pos.GetDistanceTo(washerPos);
        if (distanceByDir < 0/*over flight*/ && fullDistance > -distanceByDir/*save sqrt*/) {
          double distance = Math.Sqrt(Math.Pow(fullDistance, 2) - Math.Pow(distanceByDir,2));

          if (distance < radius) {
            return true;
          }

          return false;
        }
      }

      return false;
    }
  }
}
