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
        return isShootingWasher();
      }

      if (CarType.Jeep == car.Type) {
        return isRunTire();
      }

      return false;
    }

    public override void execute(Move move) {
      move.IsThrowProjectile = true;
    }    

    private bool isShootingWasher() {
      foreach (Car carIter in world.Cars) {
        if (carIter.IsTeammate || carIter.IsFinishedTrack || carIter.Durability <= 1.0e-9) {
          continue;
        }

        if (isEnemyOnWasherLine(carIter)) {
          return true;
        }
      }

      return false;
    }

    private bool isEnemyOnWasherLine(Car enemy) {
      Vector washerPos = new Vector(car.X, car.Y);
      Vector washerDir = Vector.sincos(car.Angle);
      Vector washerSpd = washerDir * game.WasherInitialSpeed;

      if ((new Vector(enemy.X,enemy.Y) - washerPos).Dot(washerDir) < 0) {//back car
        return false;
      }

      PhysicCar physicCar = new PhysicCar(enemy, game);

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

    private bool isRunTire() {
      PhysicCar self = null;
      List<PhysicCar> their = new List<PhysicCar>();
      List<PhysicCar> enemies = new List<PhysicCar>();

      foreach (Car carIter in world.Cars) {
        PhysicCar physicCar = new PhysicCar(carIter, game);
        if (carIter.IsTeammate) { 
          their.Add(physicCar);
        } else {
          enemies.Add(physicCar);
        }

         if (carIter.Id == car.Id) {
           self = physicCar;
          }
      }

      return isRunTire(self, their.ToArray(), enemies.ToArray());

    }

    private bool isRunTire(PhysicCar self, PhysicCar[] their, PhysicCar[] enemies) {
      Logger.instance.Assert(null != self, "Self car is null.");

      PhysicCar ignored = self;

      Vector tirePos = self.Pos;
      Vector tireSpd = self.Dir * game.TireInitialSpeed;
      double minTireSpeed = game.TireInitialSpeed * game.TireDisappearSpeedFactor;

      for (int i = 0; i < 50; i++) {
        tirePos += tireSpd;

        foreach (PhysicCar physicCar in their) {
          physicCar.Iteration(1);

          if (itersectTireWithCar(tirePos, tireSpd, physicCar, 2)) {
            if (ignored == physicCar) {
              continue;
            }

            return false;
          } 
        }

        foreach (PhysicCar physicCar in enemies) {
          physicCar.Iteration(1);

          if (itersectTireWithCar(tirePos, tireSpd, physicCar, 0.5)) {
             return physicCar.Car.Durability > 1.0e-9 && !physicCar.Car.IsFinishedTrack;
          }
        }

        Vector itersectWithMap = CollisionDetector.instance.IntersectCircleWithMap(tirePos, game.TireRadius);
        if (null != itersectWithMap) {
          ignored = null;
          tireSpd = calcTireSpeedAfterKick(tireSpd, (tirePos - itersectWithMap).Normalize());
        }

        if (tireSpd.Length < minTireSpeed) {
          return false;
        }
      }

      return false;
    }

    private bool itersectTireWithCar(Vector tirePos, Vector tireSpd, PhysicCar car, double multR = 1) {
      return CollisionDetector.instance.IntersectCarWithCircle(car.Pos, car.Dir, tirePos, game.TireRadius * multR);
    }

    private bool itersectTireWithSide(Vector tirePos, Vector tireSpd, PhysicCar car) {
      return CollisionDetector.instance.IntersectCarWithCircle(car.Pos, car.Dir, tirePos, game.TireRadius);
    }

    private Vector calcTireSpeedAfterKick(Vector speed, Vector normal) {
      const double magicFriction = 0.02;

      double friction = Math.Min(magicFriction, -speed.Dot(normal));
      return normal.Negative() * (2 * speed.Dot(normal)) + speed - (normal * friction);

      /*const double momentumTransferFactor = 1;
      double denominatorC = (speed.Negative().Cross(normal) / game.TireMass);
      Vector denominatorV = speed.Perpendicular() * denominatorC;

      double denominator = (1/game.TireMass) + normal.Dot(denominatorV);
      double impulseChange = - (1 + momentumTransferFactor) * speed.Dot(normal) / denominator;
      Vector vectorChange = normal * (impulseChange / game.TireMass);

      return speed + vectorChange;*/
    }
  }
}
