﻿using System;
using System.Collections.Generic;
using Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk.Model;

namespace RussianAICup2015Car.Sources {
  class PhysicCar {
    private const double dt = 1;

    public double WheelTurn { get { return wheelTurn; } }
    public double EnginePower { get { return enginePower; } }

    public Vector Pos { get { return pos; } }
    public Vector Speed { get { return spd; } }
    public double Angle { get { return angle; } }
    public double AngularSpeed { get { return angleSpeed; } }

    private Car car;
    private Game game;

    private double idealWheelTurn;
    private double idealEnginePower;
    private int brake = 1;
    private int nitroTicks = 0;

    private double wheelTurn;
    private double enginePower;

    private Vector pos;
    private Vector spd;
    private double angle;
    private double angleSpeed;

    private readonly double carAccel;
    private readonly double frictionMove;
    private readonly double frictionLenght;
    private readonly double frictionCross;

    private readonly double angleSpeedFactor;
    private readonly double frictionAngle;
    private readonly double frictionMaxAngleSpeed;

    public PhysicCar(Car car, Game game) {
      this.car = car;
      this.game = game;

      carAccel = carAcceleration(car, game, dt);
      frictionMove = Math.Pow(1 - game.CarMovementAirFrictionFactor, dt);
      frictionLenght = game.CarLengthwiseMovementFrictionFactor * dt;
      frictionCross = game.CarCrosswiseMovementFrictionFactor * dt;

      angleSpeedFactor = game.CarAngularSpeedFactor;
      frictionAngle = Math.Pow(1 - game.CarRotationFrictionFactor, dt);
      frictionMaxAngleSpeed = game.CarRotationFrictionFactor * dt;

      init();
    }

    public void setWheelTurn(double newWheelTurn) {
      idealWheelTurn = limit(newWheelTurn, 1);
    }

    public void setEnginePower(double newEnginePower) {
      idealEnginePower = limit(newEnginePower, 1);
    }

    public void setBrake(bool isBrake) {
      brake = isBrake ? 1 : 0;
    }

    public void useNitro() {
      nitroTicks = game.NitroDurationTicks;
    }

    public void Iteration(int ticks) {
      for (int i = 0; i < ticks / dt; i++) {
        if (nitroTicks > 0) {
          enginePower = 2;
          nitroTicks--;
        }

        Vector dir = Vector.sincos(angle);
        Vector accel = dir * (enginePower * brake * carAccel);

        pos = pos + spd * dt;
        spd = (spd + accel) * frictionMove;
        spd = spd - (dir * limit(spd.Dot(dir), frictionLenght) + dir.Perpendicular() * limit(spd.Cross(dir), frictionCross));

        double baseAngleSpeed = wheelTurn * game.CarAngularSpeedFactor * spd.Dot(dir);

        angle += angleSpeed * dt;
        angleSpeed = baseAngleSpeed - (angleSpeed - baseAngleSpeed) * frictionAngle;
        angleSpeed -= limit(angleSpeed - baseAngleSpeed, frictionMaxAngleSpeed);

        wheelTurn = signLimitChange(idealWheelTurn, wheelTurn, game.CarWheelTurnChangePerTick);
        enginePower = signLimitChange(idealEnginePower, enginePower, game.CarEnginePowerChangePerTick);
      }
    }

    private void init() {
      idealWheelTurn = wheelTurn = car.WheelTurn;
      idealEnginePower = enginePower = car.EnginePower;

      pos = new Vector(car.X, car.Y);
      spd = new Vector(car.SpeedX, car.SpeedY);
      angle = car.Angle;
      angleSpeed = car.AngularSpeed;
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