using RussianAICup2015Car.Sources.Common;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using RussianAICup2015Car.Sources.Physic;

namespace RussianAICup2015Car.Sources.Actions {
  class ShootingAction : BaseAction {
    private static readonly int maxTireRebound = 1;
    private static readonly int tireCalculateTicks = 50;

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

      PCar physicCar = new PCar(enemy, game);
      physicCar.setEnginePower(1);

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
            return !(enemy.IsTeammate || enemy.IsFinishedTrack || enemy.Durability < 1.0e-9);
          }

          return false;
        }
      }

      return false;
    }

    private bool isRunTire() {
      PCar self = null;
      List<PCar> their = new List<PCar>();
      List<PCar> enemies = new List<PCar>();

      foreach (Car carIter in world.Cars) {
        PCar physicCar = new PCar(carIter, game);
        physicCar.setEnginePower(1);
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

    private bool isRunTire(PCar self, PCar[] their, PCar[] enemies) {
      Logger.instance.Assert(null != self, "Self car is null.");

      double maxAngle = Math.Sin(Math.PI / 6);

      PCar ignored = self;

      Vector tirePos = self.Pos;
      Vector tireSpd = self.Dir * game.TireInitialSpeed;
      double minTireSpeed = game.TireInitialSpeed * game.TireDisappearSpeedFactor;

      int tireRebound = maxTireRebound + 1;
      for (int i = 0; i < tireCalculateTicks; i++) {
        Vector lastTirePos = tirePos;
        tirePos += tireSpd;

        foreach (PCar physicCar in their) {
          physicCar.Iteration(1);

          Vector collisionNormal = null;
          if (tireCollisionWithCar(tirePos, lastTirePos, physicCar, out collisionNormal, 2)) {
            if (ignored == physicCar) {
              continue;
            }

            return false;
          } 
        }

        foreach (PCar physicCar in enemies) {
          physicCar.Iteration(1);

          Vector collisionNormal = null;
          if (tireCollisionWithCar(tirePos, lastTirePos, physicCar, out collisionNormal, 0.25)) {
            if (null == collisionNormal) {
              return false;
            }

            double angle = tireSpd.Normalize().Cross(collisionNormal);
            return angle < maxAngle && physicCar.Car.Durability > 1.0e-9 && !physicCar.Car.IsFinishedTrack;
          }
        }

        Vector collisionNormalWithMap = tireCollisionWithMap(tirePos, lastTirePos);
        if (null != collisionNormalWithMap) {
          ignored = null;
          tireSpd = calcTireSpeedAfterKick(tireSpd, collisionNormalWithMap);
          tireRebound--;
        }

        if (tireSpd.Length < minTireSpeed || tireRebound < 0) {
          return false;
        }
      }

      return false;
    }

    private Vector tireCollisionWithMap(Vector pos, Vector lastPos) {
      CollisionCircle collisionTire = new CollisionCircle(pos, lastPos, game.TireRadius);

      List<CollisionInfo> collisions = CollisionDetector.CollisionsWithMap(collisionTire);

      if (!collisions.HasCollision()) {
        return null;
      }

      return collisions.AverageNormalObj1();
    }

    private bool tireCollisionWithCar(Vector tirePos, Vector tirePosLast, PCar car, out Vector normal, double multR = 1) {
      CollisionCircle collisionTire = new CollisionCircle(tirePos, tirePosLast, game.TireRadius * multR);
      CollisionRect collisionCar = new CollisionRect(car);

      CollisionInfo collision = new CollisionInfo(collisionTire, collisionCar);

      if (CollisionDetector.CheckCollision(collision)) {
        normal = collision.NormalObj1;
        return true;
      }

      normal = null;
      return false;
    }

    private Vector calcTireSpeedAfterKick(Vector speed, Vector normal) {
      const double magicFriction = 0.02;

      double friction = Math.Min(magicFriction, -speed.Dot(normal));
      return normal.Negative() * (2 * speed.Dot(normal)) + speed;

      /*const double momentumTransferFactor = 1;
      double denominatorC = (speed.Negative().Cross(normal) / game.AngularMass!!!);
      Vector denominatorV = speed.Perpendicular() * denominatorC;

      double denominator = (1/game.TireMass) + normal.Dot(denominatorV);
      double impulseChange = - (1 + momentumTransferFactor) * speed.Dot(normal) / denominator;
      Vector vectorChange = normal * (impulseChange / game.TireMass);

      return speed + vectorChange;*/
    }
  }
}
