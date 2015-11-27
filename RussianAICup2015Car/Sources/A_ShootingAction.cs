using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_ShootingAction : A_BaseAction {
    public override bool valid() {
      return checkEnemiesIntersect() && car.RemainingProjectileCooldownTicks <= 0 && car.ProjectileCount > 0;
    }

    public override void execute(Move move) {
      move.IsThrowProjectile = true;
    }    

    private bool checkEnemiesIntersect() {
      foreach (Car carIter in world.Cars) {
        if (carIter.IsTeammate || carIter.IsFinishedTrack || carIter.Durability <= 1.0e-9) {
          continue;
        }

        if (checkIntersect(carIter)) {
          return true;
        }
      }

      return false;
    }

    private bool checkIntersect(Car enemy) {
      PhysicCar physicCar = new PhysicCar(enemy, game);

      Vector washerPos = new Vector(car.X, car.Y);
      Vector washerSpd = Vector.sincos(car.Angle) * game.WasherInitialSpeed;

      double radius = Math.Min(car.Width, car.Height) * 0.5 + game.WasherRadius;

      int maxTrackDistance = Math.Min(8, (world.Height + world.Width)/2);
      int maxTicks = (int)(game.TrackTileSize * maxTrackDistance / game.WasherInitialSpeed);

      //for one 30 for two 15 other 0. 
      int ticks = maxTicks - 15 * Math.Max(0, 3 - car.RemainingProjectileCooldownTicks);

      for (int i = 0; i < maxTicks; i++) {
        physicCar.Iteration(1);
        washerPos = washerPos + washerSpd;

        if (physicCar.Pos.GetDistanceTo(washerPos) < radius) {
          return true;
        }
      }

      return false;
    }
  }
}
