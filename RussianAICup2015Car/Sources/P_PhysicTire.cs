using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

using RussianAICup2015Car.Sources.Common;

namespace RussianAICup2015Car.Sources.Physic {
  public class PTire {
    private static readonly double dt = 1;
    
    public Vector Pos { get { return pos; } }
    public Vector LastPos { get { return lastPos; } }
    public Vector Speed { get { return spd; } }

    public double Radius { get { return radius; } }

    public Projectile Tire { get { return tire; } }

    private Projectile tire;

    private Vector lastPos;
    private Vector pos;
    private Vector spd;
    private double angle;
    private double angleSpeed;

    private double radius;

    private double minTireSpeed = 0;
    private double frictionLength = 0;
    private double frictionCross = 0.01;
    private double fricitionAngleSpeed = 0;

    public PTire(Vector pos, Vector spd, Game game) {
      this.tire = null;
      this.pos = pos;
      this.lastPos = pos;
      this.radius = game.TireRadius;
      this.spd = spd;
      this.angle = spd.Angle;
      this.angleSpeed = 0;

      this.minTireSpeed = game.TireInitialSpeed * game.TireDisappearSpeedFactor;
      this.fricitionAngleSpeed = Math.Pow(1 - game.CarRotationFrictionFactor, dt);
    }

    public PTire(Projectile tire, Game game) : 
      this(new Vector(tire.X, tire.Y), new Vector(tire.SpeedX, tire.SpeedY), game) {
      this.tire = tire;

      this.radius = tire.Radius;
      this.angle = tire.Angle;
      this.angleSpeed = tire.AngularSpeed;
    }

    public PTire(PTire ptire, Game game) : this(ptire.tire, game) {
      this.pos = ptire.pos;
      this.lastPos = ptire.lastPos;
      this.spd = ptire.spd;
      this.angle = ptire.angle;
      this.angleSpeed = ptire.angleSpeed;
    }

    public void Iteration(int ticks) {
      lastPos = pos;
      for (int i = 0; i < ticks; i++) {
        pos = pos + spd * dt;

        Vector dir = Vector.sincos(angle);
        Vector frictionLengthV = dir * limit(spd.Dot(dir), frictionLength);
        Vector frictionCrossV = dir.PerpendicularLeft() * limit(spd.Cross(dir), frictionCross);
        spd = spd - frictionLengthV - frictionCrossV;

        angle += angleSpeed * dt;
        angle = angle.AngleNormalize();
        angleSpeed = angleSpeed * fricitionAngleSpeed;
      }
    }

    public bool Valid() {
      return spd.Length > minTireSpeed;
    }

    public void HitTireWitMap(Vector normal) {
      spd = normal.Negative() * (2 * spd.Dot(normal)) + spd;
      /*const double momentumTransferFactor = 1;
     double denominatorC = (speed.Negative().Cross(normal) / game.AngularMass!!!);
     Vector denominatorV = speed.Perpendicular() * denominatorC;

     double denominator = (1/game.TireMass) + normal.Dot(denominatorV);
     double impulseChange = - (1 + momentumTransferFactor) * speed.Dot(normal) / denominator;
     Vector vectorChange = normal * (impulseChange / game.TireMass);

     return speed + vectorChange;*/
    }

    private double signLimitChange(double need, double value, double add) {
      int sign = Math.Sign(need - value);
      value += sign * add;

      if (sign != Math.Sign(need - value)) {
        value = need;
      }

      return value;
    }

    private double limit(double v, double maxmin) {
      return Extensions.Limit(v, maxmin);
    }

    private static double carAcceleration(Car car, Game game, double dt) {
      switch (car.Type) {
      case CarType.Buggy:
        return game.BuggyEngineForwardPower / game.BuggyMass * dt;
      case CarType.Jeep:
        return game.JeepEngineForwardPower / game.JeepMass * dt;
      }
      return 0;
    }

  }
}
