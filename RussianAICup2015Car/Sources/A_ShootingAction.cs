using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;

namespace RussianAICup2015Car.Sources {
  class A_ShootingAction : A_BaseAction {
    public override bool valid() {
      return car.RemainingProjectileCooldownTicks <= 0 && car.ProjectileCount > 0 && enemyAhead();
    }

    public override void execute(Move move) {
      move.IsThrowProjectile = true;
    }    

    private bool enemyAhead() {
      foreach (Car carIter in world.Cars) {
        if (carIter.IsTeammate || carIter.IsFinishedTrack || 0 == carIter.Durability) {
          continue;
        }

        double distance = car.GetDistanceTo(carIter);
        if (distance > game.TrackTileSize) {
          continue;
        }

        double angle = car.GetAngleTo(carIter);
        if (Math.Abs(angle) < Math.PI / 18) {
          return true;
        }
      }

      return false;
    }
  }
}
